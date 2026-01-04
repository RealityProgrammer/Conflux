import 'hammerjs';

const MAX_OVERLAY_OPACITY = 60;

export function initializeSidebar(sidebarElement: HTMLElement, overlayElement: HTMLElement, interactingElement: HTMLElement | null): HammerManager {
    let isOpen = false, dragging = false, startX = 0, translate = -100;

    const getWidth = () => sidebarElement.getBoundingClientRect().width;

    const open = () => {
        isOpen = true;
        translate = 0;
        sidebarElement.style.transform = 'translateX(0)';
        
        overlayElement.classList.remove('invisible');
        overlayElement.style.opacity = `${MAX_OVERLAY_OPACITY}%`;
    };

    const close = () => {
        isOpen = false;
        translate = -100;
        sidebarElement.style.transform = 'translateX(-100%)';

        overlayElement.classList.add('invisible');
        overlayElement.style.opacity = '0%';
    };

    overlayElement.onclick = close;

    const hammer = new Hammer(interactingElement || document.body);
    hammer.get('pan').set({ direction: Hammer.DIRECTION_HORIZONTAL, threshold: 5 });
    
    hammer.on('panstart', (e: HammerInput) => {
        if (e.center.x > 40 && !isOpen) return;
        dragging = true;
        startX = e.center.x;

        sidebarElement.classList.remove('transform-translate', 'duration-150');
        
        overlayElement.classList.remove('invisible', 'transform-opacity', 'duration-150');
    });
    
    hammer.on('panmove', (e: HammerInput) => {
        if (!dragging) return;

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
        if (!dragging) return;
        dragging = false;

        sidebarElement.classList.add('transform-translate', 'duration-150');
        overlayElement.classList.add('transform-opacity', 'duration-150');
        translate > -50 ? open() : close();
    });
    
    return hammer;
}

export function disposeSidebar(manager: HammerManager): void {
    manager.destroy();
}

