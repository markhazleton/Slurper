const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin");

module.exports = (env, argv) => {
  const isProduction = argv.mode === "production";

  return {
    entry: {
      site: ["./src/js/vendor.js", "./src/js/site.js"],
    },
    output: {
      filename: "js/[name].js",
      path: path.resolve(__dirname, "wwwroot"),
      clean: false, // Clean is handled by the npm script
      publicPath: "/",
    },
    devtool: isProduction ? "source-map" : "eval-source-map",
    module: {
      rules: [
        {
          test: /\.js$/,
          exclude: /node_modules/,
          use: {
            loader: "babel-loader",
            options: {
              presets: ["@babel/preset-env"],
            },
          },
        },
        {
          test: /\.s[ac]ss$/i,
          use: [MiniCssExtractPlugin.loader, "css-loader", "sass-loader"],
        },
        {
          test: /\.css$/i,
          use: [MiniCssExtractPlugin.loader, "css-loader"],
        },
        {
          test: /\.(woff|woff2|eot|ttf|otf)$/i,
          type: "asset/resource",
          generator: {
            filename: "fonts/[name][ext]",
          },
        },
        {
          test: /\.(png|svg|jpg|jpeg|gif)$/i,
          type: "asset/resource",
          generator: {
            filename: "images/[name][ext]",
          },
        },
      ],
    },
    plugins: [
      new MiniCssExtractPlugin({
        filename: "css/[name].css",
      }),
      new CopyPlugin({
        patterns: [
          { from: "src/public" },
        ],
      }),
    ],
    optimization: {
      minimizer: [
        new TerserPlugin({
          terserOptions: {
            compress: {
              drop_console: isProduction,
            },
          },
          extractComments: false,
        }),
        new CssMinimizerPlugin(),
      ],
      splitChunks: {
        cacheGroups: {
          vendor: {
            test: /[\\/]node_modules[\\/]/,
            name: 'vendors',
            chunks: 'all',
            priority: 10,
          },
        },
      },
    },
    devServer: {
      static: {
        directory: path.join(__dirname, "wwwroot"),
      },
      compress: true,
      port: 9000,
      hot: true,
      proxy: {
        "/": {
          target: "http://localhost:5000",
          changeOrigin: true,
        },
      },
    },
    resolve: {
      extensions: [".js", ".json"],
      alias: {
        "@": path.resolve(__dirname, "src"),
      },
    },
    performance: {
      hints: isProduction ? "warning" : false,
      maxEntrypointSize: 600000,
      maxAssetSize: 350000,
    },
  };
};
