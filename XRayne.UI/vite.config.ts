import { defineConfig } from "vite";
import { reactRouter } from "@react-router/dev/vite";
import tailwindcss from "@tailwindcss/vite";
import mkcert from "vite-plugin-mkcert";
import svgr from "vite-plugin-svgr";
import tsconfigPaths from "vite-tsconfig-paths";

export default defineConfig({
  plugins: [mkcert(), reactRouter(), tailwindcss(), tsconfigPaths(), svgr()],
  server: {
    proxy: {
      "/api": "http://localhost:5097",
    },
    host: "127.0.0.1",
  },
});
