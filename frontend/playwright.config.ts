import {defineConfig, devices} from '@playwright/test';

/** Pruebas de punta a punta contra la web y la API ya levantadas. */
export default defineConfig({
  testDir: 'e2e',
  retries: 1,
  reporter: 'list',
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5173',
    locale: 'es-BO',
    timezoneId: 'America/La_Paz',
    trace: 'retain-on-failure',
    launchOptions: {executablePath: process.env.E2E_CHROMIUM},
  },
  projects: [{name: 'celular', use: {...devices['Pixel 7']}}],
});
