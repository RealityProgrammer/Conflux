function createInputPreviewUrl(inputElement: HTMLInputElement, fileIndex: number) : string|null {
    if (!inputElement || !inputElement.files || inputElement.files.length < fileIndex) return null;

    if (!inputElement.files) {
        return null;
    }

    const file: File | undefined = inputElement.files?.[fileIndex];
    if (!file) return null;

    return URL.createObjectURL(file);
}

declare global {
    interface Window {
        createInputPreviewUrl: (inputElement: HTMLInputElement, fileIndex: number) => string|null;
    }
}

window.createInputPreviewUrl = createInputPreviewUrl;