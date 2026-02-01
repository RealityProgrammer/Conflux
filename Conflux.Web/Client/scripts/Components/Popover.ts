import {
    computePosition, flip, shift, arrow, autoUpdate, offset, ComputePositionReturn, Placement,
} from "@floating-ui/dom";

interface Disposer {
    tooltipDisposer: () => void;
    clickDisposer: (() => void) | null;
}

function updatePosition(targetElement: Element, tooltipElement: HTMLElement, arrowElement: HTMLElement | null, tooltipPlacement: Placement, tooltipOffset: number) {
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

export function register(targetElement: Element, tooltipElement: HTMLElement, arrowElement: HTMLElement | null, tooltipPlacement: Placement, tooltipOffset: number, dotnetHelper: any, closeWhenClickOutside: boolean): Disposer | null {
    if (!targetElement || !tooltipElement) {
        return null;
    }
    
    console.log("register popover.");
    
    const disposer = autoUpdate(targetElement, tooltipElement, () => {
        updatePosition(targetElement, tooltipElement, arrowElement, tooltipPlacement, tooltipOffset);
    });
    
    let clickDisposer: (() => void) | null = null;

    if (closeWhenClickOutside) {
        const handler = async (e: PointerEvent) => {
            if (!e.target) return;
            
            const composedPath = e.composedPath();
            
            if (composedPath.includes(tooltipElement) || composedPath.includes(targetElement)) return;
            
            console.log("click outside.")
            await dotnetHelper.invokeMethodAsync('HandleOutsideClick');
        };

        document.addEventListener("pointerdown", handler, { 
            capture: true,
        });

        clickDisposer = () => document.removeEventListener("pointerdown", handler, { capture: true });
    }
    
    return {
        clickDisposer,
        tooltipDisposer: disposer,
    };
}

export function unregister(disposer: Disposer): void {
    console.log("unregister popover.");
    
    disposer.tooltipDisposer();
    disposer.clickDisposer?.();
}