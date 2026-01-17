import interact from 'interactjs';
import {Interactable} from "@interactjs/core/Interactable";

class CallScreen {
    private readonly interactable: Interactable;
    
    constructor(containerElement: HTMLElement) {
        this.interactable = interact(containerElement).on('down', _e => {
            // @ts-ignore
            containerElement.parentElement.appendChild(containerElement);
        }).resizable({
            enabled: true,
            edges: {
                left: true,
                right: true,
                top: true,
                bottom: true,
            },
            modifiers: [
                interact.modifiers.restrictEdges({
                    outer: 'parent'
                }),
                interact.modifiers.restrictSize({
                    min: { width: 240, height: 135 },
                }),
            ],
            autoScroll: {
                enabled: true,
            },
            listeners: {
                move: event => {
                    let { x, y } = event.target.dataset;

                    x = (parseFloat(x) || 0) + event.deltaRect.left;
                    y = (parseFloat(y) || 0) + event.deltaRect.top;

                    Object.assign(event.target.style, {
                        width: `${event.rect.width}px`,
                        height: `${event.rect.height}px`,
                        transform: `translate(${x}px, ${y}px)`
                    })

                    Object.assign(event.target.dataset, { x, y })
                }
            }
        }).draggable({
            inertia: {
                enabled: true,
            },
            modifiers: [
                interact.modifiers.restrictRect({
                    restriction: 'parent',
                    endOnly: true
                })
            ],
            autoScroll: {
                enabled: true,
            },

            listeners: {
                move: event => {
                    let { x, y } = event.target.dataset;

                    x = (parseFloat(x) || 0) + event.dx;
                    y = (parseFloat(y) || 0) + event.dy;

                    event.target.style.transform = 'translate(' + x + 'px, ' + y + 'px)'
                    Object.assign(event.target.dataset, { x, y });
                },
            }
        });
    }
    
    dispose = () => {
        this.interactable.unset();
    }
}

export function initialize(element: HTMLElement): CallScreen {
    return new CallScreen(element);
}

export function dispose(component: CallScreen): void {
    component.dispose();
}