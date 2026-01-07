import {animate} from "animejs";
import interact from 'interactjs'
import {Interactable} from "@interactjs/core/Interactable";

const MAX_OVERLAY_OPACITY = 60;

interface DisposeState {
    interactable: Interactable;
    resizeUnregister: () => void;
}

export function initializeSidebar(sidebarElement: HTMLElement, overlayElement: HTMLElement): DisposeState {
    const checkSurpassBreakpoint = () => {
        return window.innerWidth >= 1024; // lg breakpoint
    };

    const handleResponsiveness = () => {
        if (surpassedBreakpoint) {
            sidebarElement.style.transform = '';
            isOpen = false;
            translate = 0;
            overlayElement.classList.add('hidden');
        } else {
            sidebarElement.style.transform = isOpen ? 'translateX(0)' : 'translateX(-100%)';
        }
    }

    let isOpen = false, dragging = false, translate = -100, deltaX = 0;
   
    let surpassedBreakpoint = checkSurpassBreakpoint();
   
    const getWidth = () => sidebarElement.getBoundingClientRect().width;

    const open = () => {
        isOpen = true;
        translate = 0;

        animate(sidebarElement, {
            x: '0%',
            duration: 150,
            ease: 'linear',
        });
       
        animate(overlayElement, {
            opacity: `${MAX_OVERLAY_OPACITY / 100}`,
            duration: 150,
            ease: 'linear',
        });
       
        overlayElement.classList.remove('hidden');
    };

    const close = () => {
        isOpen = false;
        translate = -100;

        animate(sidebarElement, {
            x: '-100%',
            duration: 150,
            ease: 'linear',
            onComplete: () => {
                sidebarElement.classList.add('-translate-x-full');
            },
        });
       
        animate(overlayElement, {
            opacity: `0%`,
            duration: 150,
            ease: 'linear',
        })
       
        overlayElement.classList.add('hidden');
    };

    overlayElement.onclick = close;
   
    const interactable = interact(document.body);
    
    document.body.classList.add('dragging-container')
    
    interactable
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
            start: (e: any) => {
                if (surpassedBreakpoint || e.x0 > 100 || isOpen) return;
                
                dragging = true;
                deltaX = 0;
                
                sidebarElement.style.transform = isOpen ? 'translateX(0)' : 'translateX(-100%)';
                sidebarElement.classList.remove('-translate-x-full');
                overlayElement.classList.remove('hidden');
            },
            move: (e: any) => {
                console.log(deltaX);
                
                if (!dragging || surpassedBreakpoint) return;
                
                const width = getWidth();

                deltaX += e.dx;
                
                translate = isOpen ?
                    Math.max(-100, Math.min(0, (deltaX / width) * 100)) :
                    Math.max(-100, Math.min(0, -100 + (deltaX / width) * 100));
                sidebarElement.style.transform = `translateX(${translate}%)`;
                
                // Remap translate from -100 to 0 into opacity 0 to MAX_OVERLAY_OPACITY.
                const opacity = (translate + 100) * MAX_OVERLAY_OPACITY / 100;
                overlayElement.style.opacity = `${opacity}%`;
                overlayElement.classList.toggle('hidden', opacity <= 0);
            },
            end: (_e: any) => {
                if (!dragging || surpassedBreakpoint) return;

                dragging = false;
                console.log("stop dragging.")
                
                translate > -50 ? open() : close();
            },
        },
    })
   
    const onWindowResize = () => {
        const wasSurpassedBreakpoint = surpassedBreakpoint;
        surpassedBreakpoint = checkSurpassBreakpoint();
       
        if (wasSurpassedBreakpoint !== surpassedBreakpoint) {
            handleResponsiveness();
        }
    };
   
    window.addEventListener('resize', onWindowResize);
   
    return {
        interactable,
        resizeUnregister: () => window.removeEventListener('resize', onWindowResize),
    };
}

export function disposeSidebar(disposeState: DisposeState): void {
    disposeState.interactable.unset();
    disposeState.resizeUnregister();
}
