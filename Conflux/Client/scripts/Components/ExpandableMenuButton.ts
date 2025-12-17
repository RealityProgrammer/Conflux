const idDisposalMap = new Map<number, () => void>();
let currentId = 0;

export function initializeComponent(container: HTMLElement, dotnetHelper: any): number {
    const handler = async (e: PointerEvent) => {
        if (!e.target) return;

        if (!container.contains(e.target as Node)) {
            await dotnetHelper.invokeMethodAsync('HandleHideMenu');
        }
    };
    
    document.addEventListener("click", handler);
    
    idDisposalMap.set(++currentId, () => document.removeEventListener("click", handler));
    
    return currentId;
}

export function disposeComponent(id: number) {
    const disposer = idDisposalMap.get(id);
    
    if (disposer) {
        disposer();
    }
}