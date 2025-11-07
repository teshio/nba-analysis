import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Use VITE_PROXY_TARGET to override (e.g., https://localhost:7088 or http://localhost:5088)
const target = process.env.VITE_PROXY_TARGET || 'https://localhost:7088'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target,
        changeOrigin: true,
        secure: false, // allow self-signed dev certs
      },
    },
  },
})
