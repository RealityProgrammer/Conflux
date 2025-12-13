import { animate, JSAnimation } from "animejs";
import { registerTooltip, unregisterTooltip, initializeSensitiveInputComponents } from "../components";
import { Placement } from "@floating-ui/dom";

const animations: JSAnimation[] = [];
const tooltips = [];

export function onLoad() {
    initBackground();
    initTooltipContainers();
    initializeSensitiveInputComponents();
}
export function onDispose() {
    animations.forEach(animation => {
        animation.cancel();
    });

    tooltips.forEach(tooltipId => {
        unregisterTooltip(tooltipId);
    });
}

function initBackground() {
    for (let i = 1; i <= 5; i++) {
        const x = Math.random() * 60 - 20;
        const y = Math.random() * 60 - 20;
        const xRadius = Math.random() * 40 + 15;
        const yRadius = Math.random() * 40 + 15;
        const initialAngle = Math.random() * Math.PI * 2;
        const duration = 5000 + Math.random() * 8000;

        animations.push(animate(`.gradient-element-${i}`, {
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
        }));
    }
}

function initTooltipContainers() {
    document.querySelectorAll<HTMLElement>('[data-component-type="tooltip"]').forEach(tooltip => {
        const targetId = tooltip.dataset["tooltipFor"];
        
        if (!targetId) return;
        
        const target = document.querySelector<HTMLElement>(`[data-tooltip-target="${targetId}"]`);
        
        if (!target) return;
        
        const arrow = document.querySelector<HTMLElement>(`[data-tooltip-arrow-for="${targetId}"]`);
        const placement = tooltip.dataset["tooltipPlacement"] as Placement || 'bottom';
        const offset = parseInt(tooltip.dataset['tooltipOffset'] || '0');
        
        tooltips.push(registerTooltip(target, tooltip, arrow, placement, offset));
    });
}