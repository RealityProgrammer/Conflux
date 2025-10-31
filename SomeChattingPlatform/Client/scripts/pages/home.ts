import { tsParticles } from "@tsparticles/engine"
import { loadFull } from "tsparticles"

export async function initParticlesBackground(selectors: string): Promise<void> {
    const element = document.querySelector<HTMLElement>(selectors);

    if (!element) return;

    await loadFull(tsParticles);

    await tsParticles.load({
        id: 'particles-background',
        element: element,
        options: {
            fullScreen: true,
            particles: {
                links: {
                    enable: true,
                    color: "#E0FFFF",
                    warp: true,
                },
                move: {
                    enable: true,
                    speed: 1
                },
                number: {
                    value: 240
                },
                opacity: {
                    value: {min: 0.3, max: 1}
                },
                shape: {
                    type: ["circle", "square", "triangle", "polygon"],
                    options: {
                        polygon: [
                            {
                                sides: 5
                            },
                            {
                                sides: 6
                            },
                            {
                                sides: 8
                            }
                        ]
                    }
                },
                size: {
                    value: {min: 1, max: 3}
                },
                color: {
                    value: "#E0FFFF"
                },
            },
        }
    });
}