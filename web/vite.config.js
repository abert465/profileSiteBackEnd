import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'   // ⬅️ v4 plugin
import { fileURLToPath, URL } from 'node:url'

// Match this to your running backend
const API_TARGET = 'https://localhost:7233' // or https://localhost:7233

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),                          // ⬅️ enable Tailwind v4
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': { target: API_TARGET, changeOrigin: true, secure: false },
    },
  },
  preview: { port: 5173, strictPort: true },
})
