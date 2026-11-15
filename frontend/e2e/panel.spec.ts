import {expect, test} from '@playwright/test';

const ownerEmail = process.env.E2E_OWNER_EMAIL ?? 'dueno@barberia.local';
const ownerPassword = process.env.E2E_OWNER_PASSWORD ?? 'barberia-local';

test('el dueño entra al panel y ve la agenda y los servicios', async ({page}) => {
  await page.goto('/ingresar');
  await page.getByLabel('Correo').fill(ownerEmail);
  await page.getByLabel('Contraseña').fill(ownerPassword);
  await page.getByRole('button', {name: 'Entrar'}).click();

  await expect(page.getByRole('heading', {name: 'Agenda'})).toBeVisible();
  await page.getByRole('link', {name: 'Servicios'}).click();
  await expect(page.getByText('Degradado').first()).toBeVisible();
});

test('sin sesión el panel manda a ingresar', async ({page}) => {
  await page.goto('/panel/servicios');

  await expect(page).toHaveURL(/\/ingresar/);
});
