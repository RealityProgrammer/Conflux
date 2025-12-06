// export function growTextArea(element) {
//     this.style.height = 'auto';
//     this.style.height = (this.scrollHeight + 1.6) + 'px';
// }
//
// export function assignDotNetHelper(element, dotNetHelper) {
//     element.dotNetHelper = dotNetHelper;
// }
//
// export async function invokeMessageSend(element) {
//     await element.dotNetHelper.invokeMethodAsync('HandleMessageSend');
// }

export function initializeTextArea(element, dotNetHelper) {
    element.addEventListener('input', e => {
        element.style.height = 'auto';
        element.style.height = (element.scrollHeight + 1.6) + 'px';
    });
    
    element.addEventListener('keydown', async e => {
        if ((e.key === 'Enter' || e.key === 'NumpadEnter') && !e.shiftKey) {
            e.preventDefault();
            
            await dotNetHelper.invokeMethodAsync('HandleMessageSubmit');

            element.style.height = 'auto';
            element.style.height = (element.scrollHeight + 1.6) + 'px';
        }
    })
}