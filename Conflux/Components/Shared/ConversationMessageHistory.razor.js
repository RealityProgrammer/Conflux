export function initializeScrollContainer(element, dotNetHelper) {
    let debounceTimer;
    const threshold = 100;
    const debounceDelay = 150;
    
    element.addEventListener("scroll", async () => {
        clearTimeout(debounceTimer);
        
        debounceTimer = setTimeout(async () => {
            const distanceFromBottom = element.scrollHeight - element.scrollTop - element.clientHeight;
            const distanceFromTop = element.scrollTop;

            if (distanceFromTop <= threshold) {
                await dotNetHelper.invokeMethodAsync('HandleLoadTopMessages');
            } else if (distanceFromBottom <= threshold) {
                await dotNetHelper.invokeMethodAsync('HandleLoadBottomMessages');
            }
        }, debounceDelay);
    })
}

export function getScrollTop(element) {
    return element.scrollTop;
}

export function scrollToBottom(element) {
    element.scrollTop = element.scrollHeight;
}