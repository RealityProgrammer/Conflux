import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';
import path from "path";
import mkcert from "vite-plugin-mkcert";

export default defineConfig({
    appType: "custom",
    root: 'Client',
    plugins: [
        tailwindcss(),
        mkcert(),
    ],
    build: {
        manifest: true,
        outDir: path.join(__dirname, 'wwwroot', 'dist'),
        emptyOutDir: false,
        assetsDir: "",
        rollupOptions: {
            preserveEntrySignatures: 'strict',
            input: {
                styles: './Client/styles/input.css',
                
                home: './Client/scripts/pages/home.ts',
                login: './Client/scripts/pages/login.ts',
                register: './Client/scripts/pages/register.ts',
                components: './Client/scripts/components.ts',
            },
            output: {
                entryFileNames: (chunkInfo) => {
                    if (chunkInfo.name === 'styles') {
                        return 'css/[name].js';
                    }

                    const srcPath = chunkInfo.facadeModuleId ?? "";

                    // TODO: Figure out a way to not having to check this.
                    if (srcPath.includes('Client/scripts')) {
                        const relativePath = path.relative(
                            path.join(__dirname, 'Client', 'scripts'),
                            path.dirname(srcPath)
                        );

                        if (relativePath === '') {
                            return 'js/[name].js';
                        }

                        return path.join('js', relativePath, '[name].js').replace(/\\/g, '/');
                    }
                    
                    return 'js/[name].js';
                },
                // For asset files (CSS, images, fonts, etc.)
                assetFileNames: (assetInfo) => {
                    if (assetInfo.names) {
                        const filename = assetInfo.names[assetInfo.names.length - 1];

                        if (/css/.test(filename)) {
                            return 'css/[name].[ext]';
                        }

                        if (/js/.test(filename)) {
                            return 'js/[name].[ext]';
                        }

                        if (/png|jpe?g|svg|gif|tiff|bmp|ico/i.test(filename)) {
                            return 'assets/images/[name].[ext]';
                        }

                        if (/woff|woff2|eot|ttf|otf/.test(filename)) {
                            return 'assets/fonts/[name].[ext]';
                        }
                    }

                    return 'assets/[name].[ext]';
                },
                chunkFileNames: 'js/[name]-[hash].js',
            }
        },
    },
    server: {
        hmr: {
            host: 'localhost',
            port: 5173,
            protocol: 'ws',
        },
        watch: {
            usePolling: true
        },
        port: 5173,
    },
});