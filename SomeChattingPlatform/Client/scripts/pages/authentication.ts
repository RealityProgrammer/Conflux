import {animate} from "animejs";

export function initBackground(): void {
    for (let i = 1; i <= 5; i++) {
        const x = Math.random() * 60 - 20;
        const y = Math.random() * 60 - 20;
        const xRadius = Math.random() * 40 + 15;
        const yRadius = Math.random() * 40 + 15;
        const initialAngle = Math.random() * Math.PI * 2;
        const duration = 5000 + Math.random() * 8000;

        animate(`.gradient-element-${i}`, {
            x: {
                from: '0%',
                to: '360%',
                modifier: v => Math.cos(v * Math.PI / 180 + initialAngle) * xRadius + x,
            },
            y: {
                from: '0%',
                to: '360%',
                modifier: v => Math.sin(v * Math.PI / 180 + initialAngle) * yRadius + y,
            },
            ease: 'linear',
            duration: duration,
            loop: true,
            autoplay: true,
        });
    }
}