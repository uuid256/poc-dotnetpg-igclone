import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    host: '0.0.0.0',
    port: 3000,
    watch: {
      usePolling: true,
    },
    proxy: {
      '/api': {
        target: 'http://api:8080',
        changeOrigin: true,
      },
      '/uploads': {
        target: 'http://api:8080',
        changeOrigin: true,
      },
    },
  },
})
