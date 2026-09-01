import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Real API during development. Ignored when VITE_USE_MOCK=true.
      '/api': {
        target: process.env.VITE_API_URL ?? 'https://localhost:7001',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
