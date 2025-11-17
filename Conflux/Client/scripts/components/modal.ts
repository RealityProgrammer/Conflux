import { animate } from 'animejs';

export function animateDialog(element: HTMLElement, mode: 'start' | 'end', delay: number, duration: number, ease: string) {
    if (mode == 'start') {
        element.hidden = false;
    } else if (mode == 'end') {
        element.hidden = true;
    }
    
    // const animateProperties = {
    //     start: {
    //         top: '50%',
    //         y: '-50%',
    //     },
    //     end: {
    //         top: '100%',
    //         y: '0%',
    //     },
    // };
    //
    // animate(element, {
    //     ...animateProperties[mode],
    //     delay: delay,
    //     duration: duration,
    //     ease: ease,
    //     autoplay: true,
    //     onBegin: function() {
    //         if (mode === 'start') {
    //             element.hidden = false;
    //         }
    //     },
    //     onComplete: function() {
    //         if (mode === 'end') {
    //             element.hidden = true;
    //         }
    //     },
    // });
}