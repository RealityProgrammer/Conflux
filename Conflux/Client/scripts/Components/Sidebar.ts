import 'hammerjs';

const MAX_OVERLAY_OPACITY = 60;

interface DisposeState {
    hammer: HammerManager;
    resizeUnregister: () => void;
}

export function initializeSidebar(sidebarElement: HTMLElement, overlayElement: HTMLElement): DisposeState {
    const checkSurpassBreakpoint = () => {
        return window.innerWidth >= 1024; // lg breakpoint
    };
    
    const enableSidebarTransition = () => {
        sidebarElement.classList.add('transform-translate', 'duration-150');
    };
    
    const disableSidebarTransition = () => {
        sidebarElement.classList.remove('transform-translate', 'duration-150');
    };
    
    const enableOverlayTransition = () => {
        overlayElement.classList.add('transform-opacity', 'duration-150');
    };

    const disableOverlayTransition = () => {
        overlayElement.classList.remove('transform-opacity', 'duration-150');
    };

    let isOpen = false, dragging = false, startX = 0, translate = -100;
    
    let surpassedBreakpoint = checkSurpassBreakpoint();
    
    const getWidth = () => sidebarElement.getBoundingClientRect().width;

    const open = () => {
        if (surpassedBreakpoint || isOpen) return;
        
        isOpen = true;
        translate = 0;
        sidebarElement.classList.add('translate-x-0');
        sidebarElement.classList.remove('-translate-x-full');
        
        overlayElement.classList.remove('invisible');
        overlayElement.style.opacity = `${MAX_OVERLAY_OPACITY}%`;
    };

    const close = () => {
        isOpen = false;
        translate = -100;
        sidebarElement.classList.remove('translate-x-0');
        sidebarElement.classList.add('-translate-x-full');

        overlayElement.classList.add('invisible');
        overlayElement.style.opacity = '0%';
    };

    overlayElement.onclick = close;

    const hammer = new Hammer(document.body);
    hammer.get('pan').set({ direction: Hammer.DIRECTION_HORIZONTAL, threshold: 5 });
    
    hammer.on('panstart', (e: HammerInput) => {
        if (surpassedBreakpoint) return;
        
        if (e.center.x > 40 && !isOpen) return;
        dragging = true;
        startX = e.center.x;

        sidebarElement.classList.remove('-translate-x-full', 'translate-x-0');
        disableSidebarTransition();
        
        sidebarElement.style.transform = isOpen ? 'translateX(0)' : 'translateX(-100%)';
        
        overlayElement.classList.remove('invisible');
        disableOverlayTransition();
    });
    
    hammer.on('panmove', (e: HammerInput) => {
        if (!dragging || surpassedBreakpoint) return;

        const width = getWidth();
        const delta = e.center.x - startX;
        
        translate = isOpen ?
            Math.max(-100, Math.min(0, (delta / width) * 100)) :
            Math.max(-100, Math.min(0, -100 + (delta / width) * 100));
        
        sidebarElement.style.transform = `translateX(${translate}%)`;

        // Remap translate from -100 to 0 into opacity 0 to 40.
        const opacity = (translate + 100) * MAX_OVERLAY_OPACITY / 100;
        overlayElement.style.opacity = `${opacity}%`;
        overlayElement.classList.toggle('invisible', opacity <= 0);
    });
    
    hammer.on('panend', (_e: HammerInput) => {
        if (!dragging || surpassedBreakpoint) return;
        
        dragging = false;

        enableSidebarTransition();
        sidebarElement.style.transform = '';
        
        enableOverlayTransition();
        translate > -50 ? open() : close();
    });
    
    const onWindowResize = () => {
        const wasSurpassedBreakpoint = surpassedBreakpoint;
        surpassedBreakpoint = checkSurpassBreakpoint();
        
        if (wasSurpassedBreakpoint !== surpassedBreakpoint) {
            if (surpassedBreakpoint) {
                disableSidebarTransition();
                sidebarElement.classList.remove('-translate-x-full', 'translate-x-0');
                close();
            } else {
                sidebarElement.classList.add(isOpen ? 'translate-x-0' : '-translate-x-full');
                sidebarElement.style.transform = '';
            }
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

