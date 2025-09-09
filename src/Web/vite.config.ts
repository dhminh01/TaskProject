/// <reference types="vitest" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: "./src/tests/setup.ts",
    css: true,
  },
  build: {
    chunkSizeWarningLimit: 1000,
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ["react", "react-dom", "@apollo/client"],
          components: [
            "./src/components/form/CreateTaskForm.tsx",
            "./src/components/layout/AppContent.tsx",
            "./src/components/layout/SideBar.tsx",
            "./src/components/layout/TaskList.tsx",
            "./src/components/modal/UpdateTaskModal.tsx",
          ],
          pages: [
            "./src/pages/dashboard/DashboardPage.tsx",
            "./src/pages/task/CreateTaskPage.tsx",
            "./src/pages/task/TaskPage.tsx",
          ],
        },
      },
    },
  },
});
