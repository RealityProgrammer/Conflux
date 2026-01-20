import interact from 'interactjs';
import {Interactable} from "@interactjs/core/Interactable";

const MEDIA_STREAM_CONSTRAINTS: MediaStreamConstraints = {
    video: {
        width: 640,
        height: 480,
        facingMode: "user",
        frameRate: 24,
    },
    audio: false,
};

class CallScreen {
    private readonly interactable: Interactable;

    private readonly dotnetHelper: any;

    private peerConnection: RTCPeerConnection | undefined | null;
    
    private localMediaStream: MediaStream | undefined;
    private remoteMediaStream: MediaStream | undefined;
    private remoteVideoElement: HTMLVideoElement | undefined;
    
    private iceServers: RTCIceServer[];
    
    private userId: string;

    constructor(dotnetHelper: any, containerElement: HTMLElement, videoElement: HTMLVideoElement, userId: string, iceServers: RTCIceServer[]) {
        this.dotnetHelper = dotnetHelper;
        this.userId = userId;
        this.interactable = createInteractable(containerElement);
        this.remoteVideoElement = videoElement;
        this.iceServers = iceServers;
    };
    
    handleInitializeOutcomingCall = async () => {
        this.peerConnection = this.createPeerConnection();

        this.peerConnection.addTransceiver("video", {
            direction: "sendrecv"
        });

        try {
            this.localMediaStream = await navigator.mediaDevices.getUserMedia(MEDIA_STREAM_CONSTRAINTS);

            const videoTrack = this.localMediaStream.getVideoTracks()[0]!;

            // 2) Bind track to transceiver sender
            const videoTransceiver = this.peerConnection.getTransceivers()[0]!;

            if (!videoTransceiver) {
                throw new Error("Caller video transceiver missing");
            }

            await videoTransceiver.sender.replaceTrack(videoTrack);
            
            // this.localMediaStream.getTracks().forEach(track => {
            //     this.peerConnection!.addTrack(track, this.localMediaStream!);
            // });
            //
            // this.configurePeerConnectionTransceivers();
        } catch (err: unknown) {
            if (err instanceof Error) {
                switch (err.name) {
                    case "NotFoundError":
                        alert("Unable to open your call because no camera and/or microphone were found.");
                        break;

                    case "SecurityError":
                    case "PermissionDeniedError":
                        break;

                    default:
                        alert("Error opening your camera and/or microphone: " + err.message);
                        break;
                }
            } else {
                alert("Error opening your camera and/or microphone: " + err);
            }

            this.closeVideoCall();
            return;
        }

        try {
            const offer = await this.peerConnection.createOffer();
            await this.peerConnection.setLocalDescription(offer);

            await this.sendOffer(offer);

            dumpTransceivers(this.peerConnection);
            
            console.log(this.userId + ": create and send offer udp: " + offer.sdp);
        } catch (error) {
            window.reportError(error);
        }
    };

    handleAcceptCall = async (offer: RTCSessionDescriptionInit) => {
        try {
            this.peerConnection = this.createPeerConnection();
            await this.peerConnection.setRemoteDescription(offer);

            try {
                this.localMediaStream = await navigator.mediaDevices.getUserMedia(MEDIA_STREAM_CONSTRAINTS);

                const videoTrack = this.localMediaStream.getVideoTracks()[0]!;

                // 3) Attach to offered transceiver
                const videoTransceiver = this.peerConnection.getTransceivers()[0]!;

                if (!videoTransceiver) {
                    throw new Error("Answerer video transceiver missing");
                }

                await videoTransceiver.sender.replaceTrack(videoTrack);
                videoTransceiver.direction = "sendrecv";
                
                // this.localMediaStream.getTracks().forEach(track => {
                //     this.peerConnection!.addTrack(track, this.localMediaStream!);
                // });
                //
                // this.configurePeerConnectionTransceivers();
            } catch (err: unknown) {
                if (err instanceof Error) {
                    switch (err.name) {
                        case "NotFoundError":
                            alert("Unable to open your call because no camera and/or microphone were found.");
                            break;

                        case "SecurityError":
                        case "PermissionDeniedError":
                            break;

                        default:
                            alert("Error opening your camera and/or microphone: " + err.message);
                            break;
                    }
                } else {
                    alert("Error opening your camera and/or microphone: " + err);
                }

                this.closeVideoCall();
                return;
            }

            const answer = await this.peerConnection.createAnswer();
            
            await this.peerConnection.setLocalDescription(answer);

            await this.sendAnswer(answer);

            dumpTransceivers(this.peerConnection);
            
            console.log(this.userId + ": set and send answer: " + answer.sdp);
        } catch (error) {
            window.reportError(error);
        }
    };
    
    handleAnswer = async (answer: RTCSessionDescriptionInit) => {
        try {
            await this.peerConnection!.setRemoteDescription(answer);
            dumpTransceivers(this.peerConnection!);

            console.log(this.userId + ": received answer");
        } catch (error) {
            window.reportError(error);
        }
    };

    handleIceCandidateReceived = async (candidate: RTCIceCandidate) => {
        if (!this.peerConnection) {
            return;
        }
        
        try {
            await this.peerConnection!.addIceCandidate(candidate);
        } catch (error) {
            window.reportError(error);
        }
    }

    setupRemoteVideoStream = (videoElement: HTMLVideoElement) => {
        // this.remoteVideoElement = videoElement;
        //
        // // @ts-ignore
        // this.remoteVideoElement.srcObject = this.remoteMediaStream;
        //
        // this.remoteVideoElement.play()
        //     .then(() => console.log("remote video play successfully"))
        //     .catch((err: unknown) => {
        //         console.error("failed to play remote video: " + err);
        //     })
    };

    reportWebRTCStats = async () => {
        if (!this.peerConnection) return;

        const stats = await this.peerConnection.getStats();

        stats.forEach(r => {
            if (r.type === "inbound-rtp" && r.kind === "video") {
                console.log("Video IN:", r.bytesReceived);
            }

            if (r.type === "outbound-rtp" && r.kind === "video") {
                console.log("Video OUT:", r.bytesSent);
            }
        });
    };
    
    private createPeerConnection = (): RTCPeerConnection => {
        const connectConfiguration: RTCConfiguration = {
            iceServers: this.iceServers,
            // iceServers: [
            //     {
            //         urls: 'stun:stun.l.google.com:19302',
            //     },
            //     // {
            //     //     urls: 'turn:openrelay.metered.ca:80',
            //     //     username: 'openrelayproject',
            //     //     credential: 'openrelayproject'
            //     // },
            //     // {
            //     //     urls: 'turn:openrelay.metered.ca:443',
            //     //     username: 'openrelayproject',
            //     //     credential: 'openrelayproject'
            //     // },
            //     // {
            //     //     urls: 'turn:openrelay.metered.ca:443?transport=tcp',
            //     //     username: 'openrelayproject',
            //     //     credential: 'openrelayproject'
            //     // }
            // ],
            iceTransportPolicy: 'all',
            bundlePolicy: 'max-bundle',
            rtcpMuxPolicy: 'require',
        };

        const peerConnection = new RTCPeerConnection(connectConfiguration);
        
        console.log(this.userId + ": create peer connection");
        
        console.log(this.userId + ": set video transceiver");
        
        peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                this.sendICECandidate(event.candidate);
            }
        };
        
        peerConnection.ontrack = (event: RTCTrackEvent) => {
            console.log("peerConnection.ontrack: stream count: " + event.streams.length);
            
            if (!this.remoteMediaStream) {
                this.remoteMediaStream = new MediaStream();

                this.remoteVideoElement!.srcObject = this.remoteMediaStream;
                this.remoteVideoElement!.muted = true;
                this.remoteVideoElement!.playsInline = true;
                
                // @ts-ignore
                this.remoteVideoElement.play()
                    .then(() => console.log("remote video play successfully"))
                    .catch((err: unknown) => {
                        console.error("failed to play remote video: " + err);
                    })
            }

            this.remoteMediaStream.addTrack(event.track);

            this.dotnetHelper.invokeMethodAsync("TransitionToCallSetup");
        };

        peerConnection.oniceconnectionstatechange = () => {
            console.log("ICE connection state: ", peerConnection.iceConnectionState);
        };
        
        return peerConnection;
    };

    closeVideoCall = (): void => {
        if (this.peerConnection) {
            this.peerConnection.ontrack = null;
            this.peerConnection.onicecandidate = null;
            this.peerConnection.oniceconnectionstatechange = null;
            this.peerConnection.onsignalingstatechange = null;
            this.peerConnection.onicegatheringstatechange = null;
            this.peerConnection.onnegotiationneeded = null;

            // if (this.localMediaStream) {
            //     this.localMediaStream.getTracks().forEach((track) => track.stop());
            // }
            
            // if (this.remoteMediaStream) {
            //     this.remoteMediaStream.getTracks().forEach((track) => track.stop());
            // }

            this.peerConnection.close();
            this.peerConnection = null;
        }
    };
    
    private sendOffer = async (offer: RTCSessionDescriptionInit) => {
        await this.dotnetHelper.invokeMethodAsync("SendOffer", JSON.stringify(offer));
    }

    private sendAnswer = async (answer: RTCSessionDescriptionInit) => {
        await this.dotnetHelper.invokeMethodAsync("SendAnswer", JSON.stringify(answer));
    }
    
    private sendICECandidate = async (candidate: RTCIceCandidate) => {
        await this.dotnetHelper.invokeMethodAsync("SendIceCandidate", this.userId, JSON.stringify(candidate));
    };

    dispose = (): void => {
        this.closeVideoCall();
        this.interactable.unset();
    };
}

function createInteractable(element: HTMLElement): Interactable {
    return interact(element).on('down', _e => {
        // @ts-ignore
        element.parentElement.appendChild(element);
    }).pointerEvents({
        ignoreFrom: '.actions-container',
    }).resizable({
        enabled: true,
        edges: {
            left: true,
            right: true,
            top: true,
            bottom: true,
        },
        modifiers: [
            interact.modifiers.restrictEdges({
                outer: 'parent'
            }),
            interact.modifiers.restrictSize({
                min: { width: 240, height: 135 },
            }),
        ],
        autoScroll: {
            enabled: true,
        },
        ignoreFrom: '.actions-container',
        listeners: {
            move: event => {
                let { x, y } = event.target.dataset;

                x = (parseFloat(x) || 0) + event.deltaRect.left;
                y = (parseFloat(y) || 0) + event.deltaRect.top;

                Object.assign(event.target.style, {
                    width: `${event.rect.width}px`,
                    height: `${event.rect.height}px`,
                    transform: `translate(${x}px, ${y}px)`
                })

                Object.assign(event.target.dataset, { x, y })
            }
        }
    }).draggable({
        inertia: {
            enabled: true,
        },
        modifiers: [
            interact.modifiers.restrictRect({
                restriction: 'parent',
                endOnly: true
            })
        ],
        autoScroll: {
            enabled: true,
        },
        ignoreFrom: '.actions-container',
        listeners: {
            move: event => {
                let { x, y } = event.target.dataset;

                x = (parseFloat(x) || 0) + event.dx;
                y = (parseFloat(y) || 0) + event.dy;

                event.target.style.transform = 'translate(' + x + 'px, ' + y + 'px)'
                Object.assign(event.target.dataset, { x, y });
            },
        }
    });
}

function dumpTransceivers(pc: RTCPeerConnection) {
    const dump = pc.getTransceivers().map((t, i) => ({
        index: i,
        mid: t.mid,
        direction: t.direction,
        currentDirection: t.currentDirection,
        senderTrack: t.sender?.track?.kind || null,
        receiverTrack: t.receiver?.track?.kind || null,
    }));

    console.log("TRANSCEIVERS:", JSON.stringify(dump, null, 2));
}

export function initialize(dotnetHelper: any, element: HTMLElement, videoElement: HTMLVideoElement, userId: string, iceServers: RTCIceServer[]): CallScreen {
    return new CallScreen(dotnetHelper, element, videoElement, userId, iceServers);
}

export async function handleInitializeOutcomingCall(callScreen: CallScreen) {
    await callScreen.handleInitializeOutcomingCall();
}

export async function handleAcceptCall(callScreen: CallScreen, offer: string) {
    await callScreen.handleAcceptCall(JSON.parse(offer));
}

export async function handleCallAnswered(callScreen: CallScreen, answer: string) {
    await callScreen.handleAnswer(JSON.parse(answer));
}

export async function handleIceCandidateReceived(callScreen: CallScreen, candidate: string) {
    await callScreen.handleIceCandidateReceived(JSON.parse(candidate));
}

export function setupRemoteVideoStream(callScreen: CallScreen, videoElement: HTMLVideoElement) {
    callScreen.setupRemoteVideoStream(videoElement);
}

export async function reportWebRTCStats(callScreen: CallScreen) {
    await callScreen.reportWebRTCStats();
}

export function dispose(component: CallScreen): void {
    component.dispose();
}