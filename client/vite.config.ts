import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite';


// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react({
    babel: {
      presets: ['jotai/babel/preset'],
    },
  }),     tailwindcss()
  ],
  
  // Proxy to be able to log in when in development
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:8080",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
