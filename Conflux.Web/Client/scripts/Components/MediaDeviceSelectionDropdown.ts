type DeviceInfo = Omit<MediaDeviceInfo, "kind" | "groupId" | "toJSON">;

class DeviceDropdown {
    deviceKind: MediaDeviceKind;
    dotnetHelper: any;
    eventUnregister: (() => void) | null;
    
    constructor(dotnetHelper: any, deviceKind: MediaDeviceKind) {
        this.deviceKind = deviceKind;
        this.dotnetHelper = dotnetHelper;

        if (!navigator.mediaDevices?.enumerateDevices) {
            this.eventUnregister = null;
        } else {
            navigator.mediaDevices.addEventListener('devicechange', this.queryDevices);

            this.eventUnregister = (): void => {
                navigator.mediaDevices.removeEventListener('devicechange', this.queryDevices);
            };

            this.queryDevices();
        }
    };
    
    queryDevices = () => {
        navigator.mediaDevices.getUserMedia({
            audio: this.deviceKind === "audioinput" || this.deviceKind === "audiooutput",
            video: this.deviceKind === "videoinput"
        }).then((stream) => {
            stream.getTracks().forEach(track => track.stop());
            
            navigator.mediaDevices.enumerateDevices().then((devices) => {
                const deviceInfos: DeviceInfo[] = devices.filter((device) => device.kind === this.deviceKind).map((device) => {
                    return { deviceId: device.deviceId, label: device.label }
                });
                
                console.log(this.deviceKind + ": " + JSON.stringify(deviceInfos));
    
                this.dotnetHelper.invokeMethodAsync("OnReceivedMediaDevices", deviceInfos);
            });
        }).catch((err: unknown) => {
            console.error("Failed to get media device: " + err);
            this.dotnetHelper.invokeMethodAsync("OnMediaPermissionDenied");
        });
    };
    
    dispose = () => {
        this.eventUnregister?.();
    };
}


export function initialize(dotnetHelper: any, deviceKind: MediaDeviceKind): DeviceDropdown {
    return new DeviceDropdown(dotnetHelper, deviceKind);
}

export function dispose(component: DeviceDropdown) {
    component.dispose();
}