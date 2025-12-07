export function initializeScrollContainer(scrollContainer, dotNetHelper) {
    let debounceTimer;
    const threshold = 150;
    const debounceDelay = 150;
    
    scrollContainer.addEventListener("scroll", async () => {
        clearTimeout(debounceTimer);
        
        debounceTimer = setTimeout(async () => {
            if (scrollContainer.scrollTop <= threshold) {
                await dotNetHelper.invokeMethodAsync('HandleLoadTopMessages');
                console.log("HandleLoadTopMessages");
            } else if (scrollContainer.scrollTop + scrollContainer.clientHeight >= scrollContainer.scrollHeight - threshold) {
                await dotNetHelper.invokeMethodAsync('HandleLoadBottomMessages');
            }
        }, debounceDelay);
    })
}

export function scrollToBottom(scrollContainer) {
    scrollContainer.scrollTop = scrollContainer.scrollHeight;
    console.log("scrollToBottom: " + scrollContainer.scrollHeight);
}

export function saveScrollPosition(scrollContainer) {
    const scrollTop = scrollContainer.scrollTop;
    const scrollHeight = scrollContainer.scrollHeight;

    return { scrollTop, scrollHeight };
}

export function restoreScrollPositionForTop(scrollContainer, savedScrollPosition) {
    requestAnimationFrame(() => {
        if (savedScrollPosition) {
            const newScrollHeight = scrollContainer.scrollHeight;
            const heightIncrease = newScrollHeight - savedScrollPosition.scrollHeight;

            scrollContainer.scrollTop = savedScrollPosition.scrollTop + heightIncrease;
        }
    });
}

export function restoreScrollPositionForBottom(scrollContainer, savedScrollPosition) {
    // No need to do math, because it is automatically taken care of somehow lmao.
    
    // requestAnimationFrame(() => {
    //     if (savedScrollPosition) {
    //         // const newScrollHeight = scrollContainer.scrollHeight;
    //         // const heightIncrease = newScrollHeight - savedScrollPosition.scrollHeight;
    //
    //         // scrollContainer.scrollTop = savedScrollPosition.scrollTop + heightIncrease;
    //        
    //         // const newScrollHeight = scrollContainer.scrollHeight;
    //         // const targetDistanceFromBottom = savedScrollPosition.distanceFromBottom;
    //         //
    //         // scrollContainer.scrollTop = newScrollHeight - savedScrollPosition.clientHeight - targetDistanceFromBottom;
    //     }
    // });
}

export function jumpToMessage(scrollContainer, id) {
    const messageElement = scrollContainer.querySelector(`[data-message-id='${id}']`);
    
    if (!messageElement) return;
    
    messageElement.scrollIntoView({
        behavior: 'instant',
        block: 'center',
        container: scrollContainer,
        inline: 'nearest',
    });
}