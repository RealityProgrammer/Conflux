import {
    computePosition, flip, shift, arrow, autoUpdate, offset, ComputePositionReturn, Placement,
} from "@floating-ui/dom";

interface TooltipDisposer {
    tooltipDisposer: () => void;
    clickDisposer: (() => void) | null;
}

const disposerMap = new Map<number, TooltipDisposer>();
let tooltipId: number = 0;

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
            
            // switch (position.placement.split('-')[0]) {
            //     case 'top':
            //         arrowElement.style.transform = 'translateY(-50%)';
            //         break;
            //        
            //     case 'bottom':
            //         arrowElement.style.transform = 'translateY(50%)';
            //         break;
            //        
            //     case 'left':
            //         arrowElement.style.transform = 'translateX(-50%)';
            //         break;
            //        
            //     case 'right':
            //         arrowElement.style.transform = 'translateX(50%)';
            //         break;
            // }
        }
    });
}

export function registerTooltip(targetElement: Element, tooltipElement: HTMLElement, arrowElement: HTMLElement | null, tooltipPlacement: Placement, tooltipOffset: number, dotnetHelper: any, closeWhenClickOutside: boolean): number {
    if (!targetElement || !tooltipElement) {
        return 0;
    }
    
    const tooltipDisposer = autoUpdate(targetElement, tooltipElement, () => {
        updateTooltipPosition(targetElement, tooltipElement, arrowElement, tooltipPlacement, tooltipOffset);
    });
    
    let clickDisposer: (() => void) | null = null;

    if (closeWhenClickOutside) {
        const handler = async (e: PointerEvent) => {
            if (!e.target) return;
            
            if (!tooltipElement.contains(e.target as Node)) {
                await dotnetHelper.invokeMethodAsync('HandleOutsideClick');
            }
        };

        document.addEventListener("click", handler);

        clickDisposer = () => document.removeEventListener("click", handler);
    }
    
    disposerMap.set(++tooltipId, {
        clickDisposer,
        tooltipDisposer,
    });
    
    return tooltipId;
}

export function unregisterTooltip(id: number): void {
    const cleanup = disposerMap.get(id);
    
    if (cleanup) {
        cleanup.tooltipDisposer();
        cleanup.clickDisposer?.();
        
        disposerMap.delete(id);
    }
}