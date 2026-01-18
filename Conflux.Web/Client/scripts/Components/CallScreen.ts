import interact from 'interactjs';
import {Interactable} from "@interactjs/core/Interactable";

class CallScreen {
    private readonly interactable: Interactable;

    private readonly dotnetHelper: any;

    private peerConnection: RTCPeerConnection | undefined | null;
    
    private localMediaStream: MediaStream | undefined;
    // private remoteMediaStream: MediaStream | undefined;

    // private localVideoElement: HTMLVideoElement;
    // private audioElement: HTMLAudioElement;


    constructor(dotnetHelper: any, containerElement: HTMLElement) {
        this.dotnetHelper = dotnetHelper;

        this.interactable = createInteractable(containerElement);
        //
        // // this.peerConnection.onicecandidate = event => {
        // //     if (event.candidate) {
        // //         this.sendICECandidate(event.candidate);
        // //     }
        // // };
        // //
        // // this.peerConnection.onnegotiationneeded = (_event: Event) => {
        // //     console.log("onnegotiationneeded");
        // // };
        // //
        // // // this.peerConnection.ontrack = event => {
        // // //     if (!this.remoteMediaStream) {
        // // //         this.remoteMediaStream = new MediaStream();
        // // //         this.remoteVideoElement.srcObject = this.remoteMediaStream;
        // // //     }
        // // //
        // // //     this.remoteMediaStream.addTrack(event.track);
        // // //     this.remoteVideoElement.onloadedmetadata = _event => {
        // // //         this.remoteVideoElement.play();
        // // //     };
        // // // };
        // //
        // // this.peerConnection.onnegotiationneeded = (_event: Event): void => {
        // //     if (!this.isInitiator) {
        // //         return;
        // //     }
        // //
        // //     // @ts-ignore
        // //     this.peerConnection
        // //         .createOffer()
        // //         .then(offer => this.peerConnection?.setLocalDescription(offer))
        // //         .then(async () => {
        // //             // @ts-ignore
        // //             await this.dotnetHelper.invokeMethodAsync("SendCallOffer", JSON.stringify(this.peerConnection?.localDescription));
        // //         })
        // //         .catch(error => {
        // //             console.error("Error handling negotiation offer: " + error);
        // //         });
        // // };
    };
    
    handleInitializeOutcomingCall = async () => {
        this.peerConnection = this.createPeerConnection();

        try {
            this.localMediaStream = await navigator.mediaDevices.getUserMedia({
                video: {
                    width: {max: 1600, ideal: 1280},
                    height: {max: 900, ideal: 720},
                    facingMode: "user",
                    frameRate: 30,
                },
                audio: false,
            });
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

        const offer = await this.peerConnection.createOffer()
        await this.peerConnection.setLocalDescription(offer);
        
        this.sendOffer(offer);
    };

    handleInitializeIncomingCall = async (offerInit: RTCSessionDescriptionInit) => {
        const offer = new RTCSessionDescription(offerInit);
        this.peerConnection = this.createPeerConnection();
        
        await this.peerConnection.setRemoteDescription(offer);
        
        const answer = await this.peerConnection.createAnswer();
        await this.peerConnection.setLocalDescription(answer);
        
        this.sendAnswer(answer);
    };

    private createPeerConnection = (): RTCPeerConnection => {
        const connectConfiguration: RTCConfiguration = {
            iceServers: [{
                urls: 'stun:stun.1.google.com:19302',
            }],
        };

        const peerConnection = new RTCPeerConnection(connectConfiguration);
        
        peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                this.sendICECandidate(event.candidate);
            }
        };
        
        peerConnection.ontrack = (_event: RTCTrackEvent) => {
            console.log("peerConnection: ontrack");
        };
        
        return peerConnection;
    }

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
            //
            // if (this.remoteMediaStream) {
            //     this.remoteMediaStream.getTracks().forEach((track) => track.stop());
            // }

            this.peerConnection.close();
            this.peerConnection = null;
        }
    };
    
    private sendOffer = (offer: RTCSessionDescriptionInit): void => {
        this.dotnetHelper.invokeMethodAsync("SendOffer", JSON.stringify(offer));
    }

    private sendAnswer = (answer: RTCSessionDescriptionInit): void => {
        this.dotnetHelper.invokeMethodAsync("SendAnswer", JSON.stringify(answer));
    }
    
    private sendICECandidate = (candidate: RTCIceCandidate): void => {
        this.dotnetHelper.invokeMethodAsync("SendIceCandidate", JSON.stringify(candidate));
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

export function initialize(dotnetHelper: any, element: HTMLElement): CallScreen {
    return new CallScreen(dotnetHelper, element);
}

export async function handleInitializeOutcomingCall(callScreen: CallScreen) {
    await callScreen.handleInitializeOutcomingCall();
}

export async function handleInitializeIncomingCall(callScreen: CallScreen, offerInit: RTCSessionDescriptionInit) {
    await callScreen.handleInitializeIncomingCall(offerInit);
}

export function dispose(component: CallScreen): void {
    component.dispose();
}