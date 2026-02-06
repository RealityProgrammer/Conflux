import interact from 'interactjs';
import {Interactable} from "@interactjs/core/Interactable";

const MEDIA_STREAM_CONSTRAINTS: MediaStreamConstraints = {
    video: {
        width: 640,
        height: 480,
        facingMode: "user",
        frameRate: 24,
    },
    audio: true,
};

class CallScreen {
    private readonly interactable: Interactable;

    private readonly dotnetHelper: any;

    private peerConnection: RTCPeerConnection | null;
    
    private localMediaStream: MediaStream | undefined;
    private localVideoElement: HTMLVideoElement | undefined;
    
    private remoteMediaStream: MediaStream | undefined;
    private remoteVideoElement: HTMLVideoElement;
    
    private iceServers: RTCIceServer[];
    
    private userId: string;

    constructor(dotnetHelper: any, containerElement: HTMLElement, remoteVideoElement: HTMLVideoElement, userId: string, iceServers: RTCIceServer[]) {
        this.dotnetHelper = dotnetHelper;
        this.userId = userId;
        this.interactable = createInteractable(containerElement);
        this.iceServers = iceServers;
        this.peerConnection = null;
        this.remoteVideoElement = remoteVideoElement;
    };
    
    initializeConnectionOffer = async (localVideoElement: HTMLVideoElement) => {
        this.peerConnection = this.createPeerConnection();

        this.peerConnection.addTransceiver("video", {
            direction: "sendrecv"
        });
        
        try {
            this.localMediaStream = await navigator.mediaDevices.getUserMedia(MEDIA_STREAM_CONSTRAINTS);
            this.localVideoElement = localVideoElement;
            
            this.localMediaStream.getTracks().forEach(track => {
                this.peerConnection!.addTrack(track, this.localMediaStream!);
            });
            
            this.localVideoElement.srcObject = this.localMediaStream;
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
        } catch (error) {
            window.reportError(error);
        }
    };

    initializeConnectionAnswer = async (offer: RTCSessionDescriptionInit, localVideoElement: HTMLVideoElement) => {
        try {
            this.peerConnection = this.createPeerConnection();
            await this.peerConnection.setRemoteDescription(offer);

            try {
                this.localMediaStream = await navigator.mediaDevices.getUserMedia(MEDIA_STREAM_CONSTRAINTS);
                this.localVideoElement = localVideoElement;
                
                this.localMediaStream.getTracks().forEach(track => {
                    this.peerConnection!.addTrack(track, this.localMediaStream!);
                });
                
                this.localVideoElement.srcObject = this.localMediaStream;
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
        } catch (error) {
            window.reportError(error);
        }
    };
    
    handleAnswer = async (answer: RTCSessionDescriptionInit) => {
        try {
            await this.peerConnection!.setRemoteDescription(answer);
        } catch (error) {
            window.reportError(error);
        }
    };

    addIceCandidate = async (candidate: RTCIceCandidate) => {
        if (!this.peerConnection) {
            return;
        }
        
        try {
            await this.peerConnection!.addIceCandidate(candidate);
        } catch (err: unknown) {
            console.error("Error while adding Ice candidate: " + err);
        }
    }

    reportWebRTCStats = async () => {
        if (!this.peerConnection) return;

        const stats = await this.peerConnection.getStats();
        const statsArray: any[] = [];

        stats.forEach(report => {
            statsArray.push(report);
        });

        console.log(JSON.stringify(statsArray, null, 2));
    };
    
    private createPeerConnection = (): RTCPeerConnection => {
        const connectConfiguration: RTCConfiguration = {
            iceServers: this.iceServers,
            iceTransportPolicy: 'all',
            bundlePolicy: 'max-bundle',
            rtcpMuxPolicy: 'require',
        };

        const peerConnection = new RTCPeerConnection(connectConfiguration);
        
        peerConnection.onicecandidate = async (event: RTCPeerConnectionIceEvent) => {
            if (!event || !event.candidate) {
                return;
            }
            
            await this.sendIceCandidate(event.candidate);
        };
        
        peerConnection.ontrack = (event: RTCTrackEvent) => {
            if (!this.remoteMediaStream) {
                this.remoteMediaStream = new MediaStream();
                
                this.remoteVideoElement.srcObject = this.remoteMediaStream;
                
                // this.remoteVideoElement.load();
            }
            
            this.remoteMediaStream.addTrack(event.track);
            
            this.remoteVideoElement.play().catch((err: unknown) => {
                console.error("failed to play remote video: " + err);
            });
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

            if (this.localMediaStream) {
                this.localMediaStream.getTracks().forEach((track) => track.stop());
            }
            
            if (this.remoteMediaStream) {
                this.remoteMediaStream.getTracks().forEach((track) => track.stop());
            }
            
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
    
    private sendIceCandidate = async (candidate: RTCIceCandidate) => {
        await this.dotnetHelper.invokeMethodAsync("SendIceCandidate", this.userId, JSON.stringify(candidate));
    };

    setAudioInputDevice = async (deviceId: string) => {
        await this.replaceTrackInStream("audio", deviceId);
    };

    setAudioOutputDevice = async (deviceId: string) => {
        // Can I use double bang instead of this?
        if (typeof this.remoteVideoElement.setSinkId === 'function') {
            try {
                // Chrome expects empty string as default output device for some reason.
                const sinkId = deviceId === 'default' ? '' : deviceId;
                await this.remoteVideoElement.setSinkId(sinkId);
            } catch (err) {
                console.error("Error setting audio output device: ", err);
            }
        } else {
            console.warn("Browser does not support setSinkId.");
        }
    }

    setVideoInputDevice = async (deviceId: string) => {
        await this.replaceTrackInStream("video", deviceId);
    };

    private replaceTrackInStream = async (kind: 'audio' | 'video', deviceId: string) => {
        if (!this.localMediaStream) return;

        const constraints = {
            [kind]: deviceId === 'default' ? true : { deviceId: { exact: deviceId } }
        };

        try {
            const newStream = await navigator.mediaDevices.getUserMedia(constraints);
            const newTrack = (kind === 'audio' ? newStream.getAudioTracks()[0] : newStream.getVideoTracks()[0])!;

            const oldTrack = kind === 'audio'
                ? this.localMediaStream.getAudioTracks()[0]
                : this.localMediaStream.getVideoTracks()[0];

            if (oldTrack) {
                oldTrack.stop();
                this.localMediaStream.removeTrack(oldTrack);
            }

            this.localMediaStream.addTrack(newTrack);

            if (this.peerConnection) {
                const sender = this.peerConnection.getSenders().find(s => s.track?.kind === kind);
                if (sender) {
                    await sender.replaceTrack(newTrack);
                }
            }
        } catch (err) {
            console.error(`Failed to switch ${kind} device:`, err);
        }
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

export function initialize(dotnetHelper: any, element: HTMLElement, remoteVideoElement: HTMLVideoElement, userId: string, iceServers: RTCIceServer[]): CallScreen {
    return new CallScreen(dotnetHelper, element, remoteVideoElement, userId, iceServers);
}

export async function initializeConnectionOffer(callScreen: CallScreen, localVideoElement: HTMLVideoElement) {
    await callScreen.initializeConnectionOffer(localVideoElement);
}

export async function initializeConnectionAnswer(callScreen: CallScreen, offer: string, localVideoElement: HTMLVideoElement) {
    await callScreen.initializeConnectionAnswer(JSON.parse(offer), localVideoElement);
}

export async function handleCallAnswered(callScreen: CallScreen, answer: string) {
    await callScreen.handleAnswer(JSON.parse(answer));
}

export async function addIceCandidate(callScreen: CallScreen, candidate: string) {
    await callScreen.addIceCandidate(JSON.parse(candidate));
}

export async function reportWebRTCStats(callScreen: CallScreen) {
    await callScreen.reportWebRTCStats();
}

export async function setAudioInputDevice(callScreen: CallScreen, deviceId: string) {
    await callScreen.setAudioInputDevice(deviceId);
}

export async function setAudioOutputDevice(callScreen: CallScreen, deviceId: string) {
    await callScreen.setAudioOutputDevice(deviceId);
}

export async function setVideoInputDevice(callScreen: CallScreen, deviceId: string) {
    await callScreen.setVideoInputDevice(deviceId);
}

export function dispose(component: CallScreen): void {
    component.dispose();
}