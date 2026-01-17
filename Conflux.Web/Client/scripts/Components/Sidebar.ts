import {animate} from "animejs";
import interact from 'interactjs'
import {Interactable} from "@interactjs/core/Interactable";

const MAX_OVERLAY_OPACITY = 60;

type SidebarDirection = "ltr" | "rtl";

const checkSurpassBreakpoint = () => {
    return window.innerWidth >= 1024; // lg breakpoint
};

class Sidebar {
    private readonly sidebarElement: HTMLElement;
    private readonly overlayElement: HTMLElement;
    public direction: SidebarDirection;
    
    private isOpen: boolean;
    private dragging: boolean;
    private dragPercentage: number;
    private deltaX: number;
    private surpassedBreakpoint: boolean;
    private interactable: Interactable;
    
    constructor(sidebarElement: HTMLElement, overlayElement: HTMLElement, edgeDetector: HTMLElement, direction: SidebarDirection) {
        this.sidebarElement = sidebarElement;
        this.overlayElement = overlayElement;
        this.direction = direction;
        
        this.isOpen = this.dragging = false;
        this.dragPercentage = 0;
        this.deltaX = 0;
        
        this.surpassedBreakpoint = checkSurpassBreakpoint();
        this.handleResponsiveness();
        
        this.overlayElement.addEventListener("click", this.close);
        window.addEventListener("resize", this.onWindowResize);

        this.interactable = interact(edgeDetector)
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
                maxPerElement: 2,
            });
    }
    
    handleResponsiveness = () => {
        if (this.surpassedBreakpoint) {
            this.sidebarElement.style.transform = '';
            this.isOpen = false;
            this.dragPercentage = 0;
            this.overlayElement.classList.add('hidden');
        } else {
            this.sidebarElement.style.transform = this.isOpen ? 'translateX(0)' : `translateX(${this.direction == 'ltr' ? '-100%' : '100%'})`;
        }
    }

    getSidebarWidth = () => {
        return this.sidebarElement.getBoundingClientRect().width;
    }

    open = () => {
        this.isOpen = true;
        this.dragPercentage = 100;

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
        this.dragPercentage = 0;

        animate(this.sidebarElement, {
            x: this.direction == 'ltr' ? '-100%' : '100%',
            duration: 150,
            ease: 'linear',
        });

        animate(this.overlayElement, {
            opacity: `0%`,
            duration: 150,
            ease: 'linear',
        })

        this.overlayElement.classList.add('hidden');
    };
    
    onBeginDrag = (_e: any) => {
        if (this.surpassedBreakpoint || this.isOpen) return;
        
        this.dragging = true;
        this.deltaX = 0;

        this.overlayElement.classList.remove('hidden');
    }
    
    onDragging = (e: any) => {
        if (!this.dragging || this.surpassedBreakpoint) return;
        
        const clamp = (value: number, min: number, max: number) => {
            return Math.min(Math.max(value, min), max);
        }

        const width = this.getSidebarWidth();

        this.deltaX += e.dx;
        
        if (this.direction == 'ltr') {
            this.dragPercentage =
                clamp(this.deltaX / width, 0, 1);

            this.sidebarElement.style.transform = `translateX(${-100 + this.dragPercentage * 100}%)`;
        } else {
            this.dragPercentage =
                clamp(-this.deltaX / width, 0, 1);
            
            this.sidebarElement.style.transform = `translateX(${100 - this.dragPercentage * 100}%)`;
        }

        const opacity = this.dragPercentage * MAX_OVERLAY_OPACITY;
        this.overlayElement.style.opacity = `${opacity}%`;
        this.overlayElement.classList.toggle('hidden', opacity <= 0);
    }
    
    onEndDrag = (_e: any) => {
        if (!this.dragging || this.surpassedBreakpoint) return;

        this.dragging = false;

        this.dragPercentage >= 0.5 ? this.open() : this.close();
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

export function initializeSidebar(sidebarElement: HTMLElement, overlayElement: HTMLElement, edgeDetector: HTMLElement, direction: SidebarDirection): Sidebar {
    // document.body.classList.add('dragging-container')
    
    return new Sidebar(sidebarElement, overlayElement, edgeDetector, direction);
}

export function disposeSidebar(sidebar: Sidebar): void {
    sidebar.dispose();
}
