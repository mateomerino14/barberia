import adapter from '@sveltejs/adapter-vercel';
import {sveltekit} from '@sveltejs/kit/vite';
import {svelteTesting} from '@testing-library/svelte/vite';
import {loadEnv} from 'vite';
import {defineConfig} from 'vitest/config';

const localApi = 'http://localhost:5000';

/** Origen de la API escrito como lo pide el tipo de la política de contenido. */
type ApiOrigin = `https://${string}.${string}`;

export default defineConfig(({mode}) => {
  const env = loadEnv(mode, '.', 'VITE_');
  const api = new URL(env.VITE_API_URL || localApi).origin as ApiOrigin;
  // En desarrollo Vite recarga la página por un websocket; en producción no hace falta.
  const devSockets: ('ws:' | 'wss:')[] = [];
  if (mode === 'development') {
    devSockets.push('ws:', 'wss:');
  }
  return {
    plugins: [
      sveltekit({
        compilerOptions: {runes: true},
        adapter: adapter(),
        csp: {
          mode: 'auto',
          directives: {
            'default-src': ['self'],
            'script-src': ['self'],
            'style-src': ['self', 'unsafe-inline'],
            'img-src': ['self', 'data:', 'blob:', api],
            'font-src': ['self'],
            'connect-src': ['self', api, ...devSockets],
            'object-src': ['none'],
            'base-uri': ['self'],
            'form-action': ['self'],
            'frame-ancestors': ['none'],
          },
        },
      }),
      svelteTesting(),
    ],
    test: {
      environment: 'jsdom',
      include: ['src/**/*.test.ts'],
      setupFiles: ['./vitest.setup.ts'],
    },
  };
});
