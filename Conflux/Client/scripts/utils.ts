import { computePosition, flip, shift, arrow, offset, ComputePositionReturn, Placement } from "@floating-ui/dom";

function createInputPreviewUrl(inputElement: HTMLInputElement, fileIndex: number) : string|null {
    if (!inputElement || !inputElement.files || inputElement.files.length < fileIndex) return null;

    if (!inputElement.files) {
        return null;
    }

    const file: File | undefined = inputElement.files?.[fileIndex];
    if (!file) return null;

    return URL.createObjectURL(file);
}

function updateTooltipPosition(targetElement: Element, tooltipElement: HTMLElement, arrowElement: HTMLElement | null, tooltipPlacement: Placement, tooltipOffset: number) {
    computePosition(targetElement, tooltipElement, {
        placement: tooltipPlacement,
        middleware: [
            flip({}),
            shift(),
            offset(tooltipOffset),

            ...(arrowElement ? [ arrow({element: arrowElement, padding: 0 }) ] : []),
        ],
    }).then((position: ComputePositionReturn) => {
        tooltipElement.style.left = `${position.x}px`;
        tooltipElement.style.top = `${position.y}px`;
        tooltipElement.removeAttribute("hidden");

        const staticSide = {
            top: 'bottom',
            right: 'left',
            bottom: 'top',
            left: 'right',
            // @ts-ignore
        }[position.placement.split('-')[0]];

        if (position.middlewareData.arrow && arrowElement) {
            const {x, y} = position.middlewareData.arrow!;

            Object.assign(arrowElement.style, {
                left: x != null ? `${x}px` : '',
                top: y != null ? `${y}px` : '',
                right: '',
                bottom: '',
                [staticSide]: '-4px',
            });
        }
    });
}

function copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).catch((error: Error) => {
        console.error("Failed to copy to clipboard: " + error);
    });
}

declare global {
    interface Window {
        createInputPreviewUrl: (inputElement: HTMLInputElement, fileIndex: number) => string|null;
        updateTooltipPosition: (targetElement: Element, tooltipElement: HTMLElement, arrowElement: HTMLElement | null, tooltipPlacement: Placement, tooltipOffset: number) => void;
        copyToClipboard: (text: string) => void;
    }
}

window.createInputPreviewUrl = createInputPreviewUrl;
window.updateTooltipPosition = updateTooltipPosition;
window.copyToClipboard = copyToClipboard;