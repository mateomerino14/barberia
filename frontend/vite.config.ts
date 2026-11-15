import adapter from '@sveltejs/adapter-vercel';
import {sveltekit} from '@sveltejs/kit/vite';
import {svelteTesting} from '@testing-library/svelte/vite';
import {defineConfig} from 'vitest/config';

export default defineConfig({
  plugins: [
    sveltekit({compilerOptions: {runes: true}, adapter: adapter()}),
    svelteTesting(),
  ],
  test: {
    environment: 'jsdom',
    include: ['src/**/*.test.ts'],
    setupFiles: ['./vitest.setup.ts'],
  },
});
