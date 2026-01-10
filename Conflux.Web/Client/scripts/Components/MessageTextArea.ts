export function initializeTextArea(element: HTMLTextAreaElement, dotNetHelper: any) {
    element.addEventListener('input', _e => {
        resizeTextArea(element);
    });
    
    element.addEventListener('keydown', async e => {
        switch (e.key) {
            case 'Enter':
            case 'NumpadEnter':
                if (e.shiftKey) break;
                
                e.preventDefault();

                await dotNetHelper.invokeMethodAsync('HandleEnterSubmit');

                resizeTextArea(element);
                break;
                
            case 'Escape':
                await dotNetHelper.invokeMethodAsync('HandleEscapeRequest');
                break;
        }
    })
}

export function resizeTextArea(element: HTMLTextAreaElement): void {
    element.style.height = 'auto';
    element.style.height = (element.scrollHeight + 1.6) + 'px';
}