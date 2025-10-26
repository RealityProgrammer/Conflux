import { tsParticles } from "@tsparticles/engine"
import { loadFull } from "tsparticles"

initParticlesBackground('js-particles-background').catch((error) => {
    console.log("Failed to initialize particles background. Error: " + error);
});

export async function initParticlesBackground(elementId: string): Promise<void> {
    await loadFull(tsParticles);

    await tsParticles.load({
        id: elementId,
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