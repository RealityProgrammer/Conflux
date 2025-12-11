export function createFilePreviewUrl(inputElement, fileIndex) {
    return URL.createObjectURL(inputElement.files[fileIndex]);
}