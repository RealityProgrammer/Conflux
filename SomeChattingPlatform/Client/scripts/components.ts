import { computePosition, flip, shift, arrow, autoUpdate, offset, ComputePositionReturn, Placement } from "@floating-ui/dom";

const tooltipInstances = new Map<number, () => void>();
let tooltipId = 0;

function updateTooltipPosition(targetElement: Element, tooltipElement: HTMLElement, arrowElement: HTMLElement, tooltipPlacement: Placement, tooltipOffset: number) {
    computePosition(targetElement, tooltipElement, {
        placement: tooltipPlacement,
        middleware: [
            flip({}),
            shift(),
            arrow({
                element: arrowElement,
                padding: 0,
            }),
            offset(tooltipOffset),
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

        if (position.middlewareData.arrow) {
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


export function registerTooltip(targetElement: Element, tooltipElement: HTMLElement, arrowElement: HTMLElement, tooltipPlacement: Placement, tooltipOffset: number): number {
    const cleanup = autoUpdate(targetElement, tooltipElement, () => {
        updateTooltipPosition(targetElement, tooltipElement, arrowElement, tooltipPlacement, tooltipOffset);
    });

    tooltipInstances.set(++tooltipId, cleanup);
    
    return tooltipId;
}

export function unregisterTooltip(id: number): void {
    const cleanup = tooltipInstances.get(id);
    if (cleanup) {
        cleanup();
        tooltipInstances.delete(id);
    }
}