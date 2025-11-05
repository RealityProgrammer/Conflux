import { tsParticles, Container } from "@tsparticles/engine"
import { loadFull } from "tsparticles"

let particleContainer: Container | undefined;

export function initParticlesBackground(element: HTMLElement): void{
    if (!element || particleContainer != undefined) return;

    loadFull(tsParticles).then(() => {
        tsParticles.load({
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
        }).then((container: Container | undefined) => {
            particleContainer = container;
            console.log("Particles background initialized successfully.");
        }).catch((error) => {
            console.error("Failed to initialize particles background: " + error);
        });
    });
}

export function disposeParticlesBackground() {
    if (!particleContainer) return;
    
    particleContainer.destroy(true);
    particleContainer = undefined;
}