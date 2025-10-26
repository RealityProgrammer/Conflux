const path = require('path');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = (env, argv) => {
    const mode = argv.mode || 'development';
    
    return {
        mode,
        devtool: mode === 'development' ? 'source-map' : false,
        entry: {
            'home': './ts/home.ts',
            'chatting': './ts/chatting.ts',
            'style': './sass/app.scss',
            'reset-style': './sass/reset.scss',
        },
        module: {
            rules: [
                {
                    test: /\.ts$/,
                    use: {
                        loader: 'ts-loader',
                    },
                    exclude: /node_modules/,
                },
                {
                    test: /\.js$/,
                    use: {
                        loader: 'babel-loader',
                        options: {
                            presets: ['@babel/preset-env']
                        },
                    },
                    exclude: /node_modules/,
                },
                {
                    test: /\.s[ac]ss$/i,
                    use: [
                        MiniCssExtractPlugin.loader,
                        // "style-loader",
                        "css-loader",
                        "sass-loader"
                    ],
                },
            ]
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: '../css/[name].css',
            }),
        ],
        output: {
            path: path.resolve(__dirname, '../wwwroot/js'),
            filename: '[name].js'
        },
        resolve: { extensions: ['.ts', '.js', '.scss', '.sass'] },
        devServer: {
            static: path.join(__dirname, '../wwwroot'),
            compress: true,
            port: 9000,
            hot: true,
            liveReload: true,
            devMiddleware: {
                writeToDisk: false,
            }
        },
    };
};