import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';
import path from "path";
import mkcert from "vite-plugin-mkcert";
import fs from 'fs';

export default defineConfig({
    appType: "custom",
    root: path.resolve(__dirname, "Client"),
    plugins: [
        tailwindcss(),
        mkcert(),
    ],
    resolve: {
        alias: { "~": __dirname }
    },
    build: {
        manifest: true,
        outDir: path.join(__dirname, 'wwwroot'),
        emptyOutDir: false,
        assetsDir: "",
        rollupOptions: {
            preserveEntrySignatures: 'strict',
            input: {
                styles: './Client/styles/input.css',
                
                ...getJavascriptEntrypoints()
            },
            output: {
                entryFileNames: "js/[name].js",
                chunkFileNames: "js/[name].js",
                assetFileNames: (info) => {
                    const name = info.names[info.names.length - 1];
                    
                    if (name) {
                        // If the file is a CSS file, save it to the "css" folder
                        if (/\.css$/.test(name)) {
                            return "styles/[name].[ext]";
                        }

                        // If the file is an image file, save it to the "img" folder
                        if (/\.(png|jpe?g|gif|svg|webp|avif)$/.test(name)) {
                            return "img/[name][extname]";
                        }

                        // If the file is any other type of file, save it to the "assets" folder 
                        return "assets/[name][extname]";
                    } else {

                        // If the file name is not specified, save it to the output directory
                        return "[name][extname]";
                    }
                },
                manualChunks: (id) => {
                    if (id.includes('node_modules')) {
                        return 'lib';
                    }
                }
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

function getJavascriptEntrypoints() {
    const entries = {}
    const scriptsDir = path.join(__dirname, 'Client', 'scripts')

    // Function to recursively find .js files
    function findJsFiles(dir) {
        const files = fs.readdirSync(dir, { withFileTypes: true })

        files.forEach(file => {
            const fullPath = path.join(dir, file.name)

            if (file.isDirectory()) {
                if (!['node_modules', 'bin', 'obj', 'wwwroot'].includes(file.name)) {
                    findJsFiles(fullPath)
                }
            } else if (file.name.endsWith('.js') || file.name.endsWith('.ts')) {
                // Get path relative to scripts directory
                const relativePath = path.relative(scriptsDir, fullPath)
                const entryName = relativePath.replace(/\.ts$/, '').replace(/\.js$/, '')
                entries[entryName] = fullPath
            }
        })
    }

    // Start searching from scripts directory
    findJsFiles(scriptsDir)

    return entries
}