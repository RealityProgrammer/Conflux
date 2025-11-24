export function previewImage(inputElement: HTMLInputElement, imageElement: HTMLImageElement): string | null {
    if (!inputElement || !imageElement) return null;
    
    if (!inputElement.files) {
        return null;
    }
    
    const file: File | undefined = inputElement.files?.[0];
    if (!file) return null;
    
    const url = URL.createObjectURL(file);
    // imageElement.addEventListener('load', () => {
    //     URL.revokeObjectURL(url);
    //     console.log('revokeObjectURL.');
    // }, { once: true });
    imageElement.src = url;
    
    return url;
}