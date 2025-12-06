export function initializeScrollContainer(element, dotNetHelper) {
    let debounceTimer;
    const threshold = 150;
    const debounceDelay = 150;
    
    element.addEventListener("scroll", async () => {
        clearTimeout(debounceTimer);
        
        debounceTimer = setTimeout(async () => {
            const distanceFromTop = element.scrollTop;
            const distanceFromBottom = element.scrollHeight - element.scrollTop - element.clientHeight;

            if (distanceFromTop <= threshold) {
                await dotNetHelper.invokeMethodAsync('HandleLoadTopMessages');
            } else if (distanceFromBottom <= threshold) {
                await dotNetHelper.invokeMethodAsync('HandleLoadBottomMessages');
            }
        }, debounceDelay);
    })
}

export function scrollToBottom(element) {
    element.scrollTop = element.scrollHeight;
}

export function saveScrollPosition(element) {
    console.log("saveScrollPosition");
    
    const scrollTop = element.scrollTop;
    const scrollHeight = element.scrollHeight;

    return { scrollTop, scrollHeight };
}

export function restoreScrollPosition(element, savedScrollPosition) {
    requestAnimationFrame(() => {
        console.log("restoreScrollPosition");
        
        if (savedScrollPosition) {
            const newScrollHeight = element.scrollHeight;
            const heightIncrease = newScrollHeight - savedScrollPosition.scrollHeight;

            element.scrollTop = savedScrollPosition.scrollTop + heightIncrease;
        }
    });
}