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
        outDir: path.join(__dirname, 'wwwroot'),
        emptyOutDir: false,
        assetsDir: "",
        rollupOptions: {
            preserveEntrySignatures: 'strict',
            input: {
                styles: './Client/styles/input.css',
                home: './Client/scripts/pages/home.ts',
                authentication: './Client/scripts/pages/authentication.ts',
                components: './Client/scripts/components.ts',
            },
            output: {
                entryFileNames: chunkInfo => {
                    const srcPath = chunkInfo.facadeModuleId ?? "";
                    const parsed = path.parse(srcPath);

                    const relDir = parsed.dir.replace(/^.*?scripts[\\/]/, "");

                    return path.posix.join("js", relDir, `${parsed.name}.js`);
                },
                assetFileNames: assetInfo => {
                    if (assetInfo.names) {
                        const filename = assetInfo.names[assetInfo.names.length - 1];

                        if (/\.css$/.test(filename)) {
                            return path.join("css", ...assetInfo.names.slice(0, -1), "[name].[ext]")
                        }

                        if (/\.js$/.test(filename)) {
                            return path.join("js", ...assetInfo.names.slice(0, -1), "[name].[ext]");
                        }

                        if (/\.(png|jpe?g|gif|svg|webp|avif)$/.test(filename)) {
                            return "img/[name][extname]";
                        }

                        return "assets/[name][extname]";
                    }
                    
                    return '[name][extname]';
                },
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