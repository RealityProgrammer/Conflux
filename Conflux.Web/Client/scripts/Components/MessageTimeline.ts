export function initializeScrollContainer(scrollContainer: Element, dotNetHelper: any): void {
    let debounceTimer: number;
    const threshold = 150;
    const debounceDelay = 150;

    scrollContainer.addEventListener("scroll", async () => {
        clearTimeout(debounceTimer);

        debounceTimer = setTimeout(async () => {
            if (scrollContainer.scrollTop <= threshold) {
                await dotNetHelper.invokeMethodAsync('HandleLoadTopMessages');
            } else if (scrollContainer.scrollTop + scrollContainer.clientHeight >= scrollContainer.scrollHeight - threshold) {
                await dotNetHelper.invokeMethodAsync('HandleLoadBottomMessages');
            }
        }, debounceDelay);
    })
}

export function scrollToBottom(scrollContainer: Element): void {
    scrollContainer.scrollTop = scrollContainer.scrollHeight;
}

export function saveScrollPosition(scrollContainer: Element): { scrollTop: number, scrollHeight: number } {
    const scrollTop = scrollContainer.scrollTop;
    const scrollHeight = scrollContainer.scrollHeight;

    return { scrollTop, scrollHeight };
}

export function restoreScrollPositionForTop(scrollContainer: Element, savedScrollPosition: { scrollTop: number, scrollHeight: number }): void {
    requestAnimationFrame(() => {
        if (savedScrollPosition) {
            const newScrollHeight = scrollContainer.scrollHeight;
            const heightIncrease = newScrollHeight - savedScrollPosition.scrollHeight;

            scrollContainer.scrollTop = savedScrollPosition.scrollTop + heightIncrease;
        }
    });
}

export function restoreScrollPositionForBottom(_scrollContainer: Element, _savedScrollPosition: { scrollTop: number, scrollHeight: number }): void {
    // No need to do math, because it is automatically taken care of somehow lmao.
}

export function jumpToMessage(scrollContainer: HTMLElement, id: string) {
    const messageElement = scrollContainer.querySelector(`[data-message-id='${id}']`);

    if (!messageElement) return;

    messageElement.scrollIntoView({
        behavior: 'instant',
        block: 'center',
        container: scrollContainer,
        inline: 'nearest',
    });
}

export function shouldMaintainScrollBottom(scrollContainer: HTMLElement): boolean {
    return Math.abs(scrollContainer.scrollTop - scrollContainer.scrollHeight + scrollContainer.offsetHeight) <= 0.1;
}