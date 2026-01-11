import {animate} from "animejs";
import interact from 'interactjs'
import {Interactable} from "@interactjs/core/Interactable";

const MAX_OVERLAY_OPACITY = 60;

const checkSurpassBreakpoint = () => {
    return window.innerWidth >= 1024; // lg breakpoint
};

class Sidebar {
    private readonly sidebarElement: HTMLElement;
    private readonly overlayElement: HTMLElement;
    private isOpen: boolean;
    private dragging: boolean;
    private translate: number;
    private deltaX: number;
    private surpassedBreakpoint: boolean;
    private interactable: Interactable;
    
    constructor(sidebarElement: HTMLElement, overlayElement: HTMLElement) {
        this.sidebarElement = sidebarElement;
        this.overlayElement = overlayElement;
        
        this.isOpen = this.dragging = false;
        this.translate = -100;
        this.deltaX = 0;
        
        this.surpassedBreakpoint = checkSurpassBreakpoint();
        
        this.overlayElement.addEventListener("click", this.close);
        window.addEventListener("resize", this.onWindowResize);

        this.interactable = interact(document.body)
            .styleCursor(false)
            .draggable({
                inertia: false,
                autoScroll: true,
                modifiers: [
                    // interact.modifiers.restrict({
                    //     restriction: 'self',
                    //     endOnly: false,
                    //     elementRect: { top: 0, left: 0, bottom: 1, right: 1 }
                    // })
                ],
                listeners: {
                    start: this.onBeginDrag,
                    move: this.onDragging,
                    end: this.onEndDrag,
                },
            });
    }
    
    handleResponsiveness = () => {
        if (this.surpassedBreakpoint) {
            this.sidebarElement.style.transform = '';
            this.isOpen = false;
            this.translate = 0;
            this.overlayElement.classList.add('hidden');
        } else {
            this.sidebarElement.style.transform = this.isOpen ? 'translateX(0)' : 'translateX(-100%)';
        }
    }

    getSidebarWidth = () => {
        return this.sidebarElement.getBoundingClientRect().width;
    }

    open = () => {
        this.isOpen = true;
        this.translate = 0;

        animate(this.sidebarElement, {
            x: '0%',
            duration: 150,
            ease: 'linear',
        });

        animate(this.overlayElement, {
            opacity: `${MAX_OVERLAY_OPACITY / 100}`,
            duration: 150,
            ease: 'linear',
        });

        this.overlayElement.classList.remove('hidden');
    };

    close = () => {
        this.isOpen = false;
        this.translate = -100;

        animate(this.sidebarElement, {
            x: '-100%',
            duration: 150,
            ease: 'linear',
            onComplete: () => {
                this.sidebarElement.classList.add('-translate-x-full');
            },
        });

        animate(this.overlayElement, {
            opacity: `0%`,
            duration: 150,
            ease: 'linear',
        })

        this.overlayElement.classList.add('hidden');
    };
    
    onBeginDrag = (e: any) => {
        if (this.surpassedBreakpoint || e.x0 > 100 || this.isOpen) return;

        this.dragging = true;
        this.deltaX = 0;

        this.sidebarElement.style.transform = this.isOpen ? 'translateX(0)' : 'translateX(-100%)';
        this.sidebarElement.classList.remove('-translate-x-full');
        this.overlayElement.classList.remove('hidden');
    }
    
    onDragging = (e: any) => {
        if (!this.dragging || this.surpassedBreakpoint) return;

        const width = this.getSidebarWidth();

        this.deltaX += e.dx;

        this.translate = this.isOpen ?
            Math.max(-100, Math.min(0, (this.deltaX / width) * 100)) :
            Math.max(-100, Math.min(0, -100 + (this.deltaX / width) * 100));
        this.sidebarElement.style.transform = `translateX(${this.translate}%)`;

        // Remap translate from -100 to 0 into opacity 0 to MAX_OVERLAY_OPACITY.
        const opacity = (this.translate + 100) * MAX_OVERLAY_OPACITY / 100;
        this.overlayElement.style.opacity = `${opacity}%`;
        this.overlayElement.classList.toggle('hidden', opacity <= 0);
    }
    
    onEndDrag = (_e: any) => {
        if (!this.dragging || this.surpassedBreakpoint) return;

        this.dragging = false;

        this.translate > -50 ? this.open() : this.close();
    }

    onWindowResize = () => {
        const wasSurpassedBreakpoint = this.surpassedBreakpoint;
        this.surpassedBreakpoint = checkSurpassBreakpoint();

        if (wasSurpassedBreakpoint !== this.surpassedBreakpoint) {
            this.handleResponsiveness();
        }
    };
    
    dispose = () => {
        this.interactable.unset();
        this.overlayElement.removeEventListener("click", this.close);
        window.removeEventListener("resize", this.onWindowResize);
    }
}

export function initializeSidebar(sidebarElement: HTMLElement, overlayElement: HTMLElement): Sidebar {
    document.body.classList.add('dragging-container')
    
    return new Sidebar(sidebarElement, overlayElement);
}

export function disposeSidebar(sidebar: Sidebar): void {
    sidebar.dispose();
}
