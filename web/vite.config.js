import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

// The source lives in app/, so the build has to climb out of it to reach web/dist, which is
// what the API serves.
export default defineConfig({
  root: "app",
  base: "./",
  plugins: [react(), tailwindcss()],
  build: { outDir: "../dist" },
  server: {
    proxy: {
      "/runs": "http://localhost:5023",
      "/content": "http://localhost:5023",
    },
  },
});
