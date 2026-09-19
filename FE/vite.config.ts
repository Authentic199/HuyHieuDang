import react from '@vitejs/plugin-react';
import { defineConfig, loadEnv } from 'vite';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  // Khi chạy dev, /api được chuyển tiếp sang BE để tránh vướng CORS.
  const apiProxyTarget = env.VITE_DEV_API_PROXY ?? 'http://localhost:8080';

  return {
    plugins: [react()],
    server: {
      port: 5173,
      host: true,
      proxy: {
        '/api': {
          target: apiProxyTarget,
          changeOrigin: true,
        },
      },
    },
    preview: { port: 4173, host: true },
    build: {
      outDir: 'dist',
      sourcemap: mode !== 'production',
    },
  };
});
