import { computePosition, flip, shift, arrow, autoUpdate, offset, ComputePositionReturn, Placement } from "@floating-ui/dom";
import { animate } from 'animejs';

const tooltipInstances = new Map<number, () => void>();
let tooltipId = 0;

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
        }
    });
}

export function registerTooltip(targetElement: Element, tooltipElement: HTMLElement, arrowElement: HTMLElement | null, tooltipPlacement: Placement, tooltipOffset: number): number {
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

export function animateModalContainer(element: HTMLElement, state: 'fly-in' | 'fly-out', duration: number): void {
    switch (state) {
        case 'fly-in':
            element.style.top = '0';
            element.style.transform = 'translateY(-100%)'
            
            animate(element, {
                top: '50%',
                y: '-50%',
                duration: duration,
                easing: "linear",
                onBegin: _ => {
                    element.hidden = false;
                },
            });
            break;
            
        case 'fly-out':
            element.style.top = '50%';
            element.style.transform = 'translateY(-50%)'
            
            element.hidden = false;

            animate(element, {
                top: '100%',
                y: '0%',
                duration: duration,
                easing: "linear",
                onComplete: _ => element.hidden = true,
            });
            break;
    }
}

export function initializeSensitiveInputComponents() {
    document.querySelectorAll<HTMLElement>('[data-component-type="sensitive-input"]').forEach(initializeSensitiveInputComponent);
}

function initializeSensitiveInputComponent(element: HTMLElement) {
    const input = element.querySelector('input');
    if (!input) return;

    const button = element.querySelector<Element>('button');
    if (!button) return;

    const contents = button.querySelectorAll<HTMLDivElement>('div');
    if (contents.length < 2) return;

    const initialShow = element.dataset['sensitiveInputShow'];

    if (!initialShow || initialShow === 'false') {
        element.dataset['sensitiveInputShow'] = 'false';

        // @ts-ignore
        contents[0].hidden = false;
        // @ts-ignore
        contents[1].hidden = true;

        input.type = 'password';
    } else {
        // @ts-ignore
        contents[0].hidden = true;
        // @ts-ignore
        contents[1].hidden = false;

        input.type = 'text';
    }

    button.addEventListener('click', () => {
        const nextAction = !(element.dataset['sensitiveInputShow'] === 'true');

        // @ts-ignore
        contents[0].hidden = nextAction;
        // @ts-ignore
        contents[1].hidden = !nextAction;

        input.type = nextAction ? 'text' : 'password';

        element.dataset['sensitiveInputShow'] = `${nextAction}`;
    });
}