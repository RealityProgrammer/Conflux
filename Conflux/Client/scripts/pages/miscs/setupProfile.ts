export function previewImage(inputElement: HTMLInputElement, imageElement: HTMLImageElement): string | null {
    if (!inputElement || !imageElement) return null;

    if (!inputElement.files) {
        return null;
    }

    console.log(inputElement);

    const file: File | undefined = inputElement.files?.[0];
    if (!file) return null;

    const url = URL.createObjectURL(file);
    imageElement.addEventListener('load', () => URL.revokeObjectURL(url), { once: true });
    imageElement.src = url;

    return url;
}

export function revokePreviewImageUrl(url: string) {
    URL.revokeObjectURL(url);
}