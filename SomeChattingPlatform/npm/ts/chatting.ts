import { createTimeline } from "animejs";

const timeline = createTimeline({
    loop: false,
    autoplay: false,
}).add('#js-server-navigation__element-container', {
    translateX: {
        from: '-150%',
        to: 0,
    },
    ease: 'easeOut',
    duration: 250,
}).add('#js-server-navigation__element-container', {
    height: {
        from: '1.5rem',
        to: '75vh',
    },
    ease: 'easeOut',
    duration: 350,
    delay: 100,
});

function animateServerElementsContainer(): void {
    timeline.restart();
    timeline.play();
}

declare global {
    interface Window {
        animateServerElementsContainer: () => void;
    }
}

window.animateServerElementsContainer = animateServerElementsContainer;