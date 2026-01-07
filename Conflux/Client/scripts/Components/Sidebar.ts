import 'hammerjs';
import {animate} from "animejs";

const MAX_OVERLAY_OPACITY = 60;

interface DisposeState {
    hammer: HammerManager;
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

    let isOpen = false, dragging = false, startX = 0, translate = -100;
    
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

    const hammer = new Hammer(document.body);
    hammer.get('pan').set({ direction: Hammer.DIRECTION_HORIZONTAL, threshold: 5 });
    
    hammer.on('panstart', (e: HammerInput) => {
        if (surpassedBreakpoint) return;
        
        if (e.center.x > 40 || isOpen) return;
        dragging = true;
        startX = e.center.x;

        sidebarElement.style.transform = isOpen ? 'translateX(0)' : 'translateX(-100%)';
        
        sidebarElement.classList.remove('-translate-x-full');
        overlayElement.classList.remove('hidden');
    });
    
    hammer.on('panmove', (e: HammerInput) => {
        if (!dragging || surpassedBreakpoint) return;

        const width = getWidth();
        const delta = e.center.x - startX;
        
        translate = isOpen ?
            Math.max(-100, Math.min(0, (delta / width) * 100)) :
            Math.max(-100, Math.min(0, -100 + (delta / width) * 100));
        
        sidebarElement.style.transform = `translateX(${translate}%)`;

        // Remap translate from -100 to 0 into opacity 0 to MAX_OVERLAY_OPACITY.
        const opacity = (translate + 100) * MAX_OVERLAY_OPACITY / 100;
        overlayElement.style.opacity = `${opacity}%`;
        overlayElement.classList.toggle('hidden', opacity <= 0);
    });
    
    hammer.on('panend', (_e: HammerInput) => {
        if (!dragging || surpassedBreakpoint) return;
        
        dragging = false;

        translate > -50 ? open() : close();
    });
    
    const onWindowResize = () => {
        const wasSurpassedBreakpoint = surpassedBreakpoint;
        surpassedBreakpoint = checkSurpassBreakpoint();
        
        if (wasSurpassedBreakpoint !== surpassedBreakpoint) {
            handleResponsiveness();
        }
    };
    
    window.addEventListener('resize', onWindowResize);
    
    return {
        hammer,
        resizeUnregister: () => window.removeEventListener('resize', onWindowResize),
    };
}

export function disposeSidebar(disposeState: DisposeState): void {
    disposeState.hammer.destroy();
    disposeState.resizeUnregister();
}

