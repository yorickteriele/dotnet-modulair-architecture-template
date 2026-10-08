import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwind from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwind()],
  server: {
    port: 5000,
    strictPort: true,
    proxy: { '/api': process.env.API_PROXY_TARGET ?? 'http://127.0.0.1:5001' },
  },
})
