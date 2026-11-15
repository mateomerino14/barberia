import {expect, test} from '@playwright/test';

const ownerEmail = process.env.E2E_OWNER_EMAIL ?? 'dueno@barberia.local';
const ownerPassword = process.env.E2E_OWNER_PASSWORD ?? 'barberia-local';
const oneHourMs = 3_600_000;
// Cuadrado rojo de 40 px en PNG, para probar la subida de imágenes sin archivos aparte.
const redSquare = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAACgAAAAoCAIAAAADnC86AAAAQklEQVR4Xu3QwQnAIADAQOsgfXZ/6ILiEvHh3QKBPP/3jhPmiegmnLE6Y3XG6ozVGaszVmeszlidsTpjdcbqzH2rF0TiAUdfLuiVAAAAAElFTkSuQmCC', 'base64');

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

test('una sesión que la API ya no acepta vuelve a ingresar', async ({page}) => {
  await page.goto('/ingresar');
  await page.evaluate((lifetime) => {
    const expiresAt = new Date(Date.now() + lifetime).toISOString();
    const fake = {token: 'pase-falso', expiresAt, email: 'nadie@barberia.local', role: 'Owner', barberId: null};
    localStorage.setItem('barberia.session', JSON.stringify(fake));
  }, oneHourMs);
  await page.goto('/panel/agenda');

  await expect(page).toHaveURL(/\/ingresar/);
});

test('el dueño sube el logo y aparece en la presentación', async ({page}) => {
  await page.goto('/ingresar');
  await page.getByLabel('Correo').fill(ownerEmail);
  await page.getByLabel('Contraseña').fill(ownerPassword);
  await page.getByRole('button', {name: 'Entrar'}).click();
  await expect(page.getByRole('heading', {name: 'Agenda'})).toBeVisible();

  await page.goto('/panel/ajustes');
  await page.locator('label', {hasText: 'Subir logo'}).locator('input').setInputFiles({
    name: 'logo.png',
    mimeType: 'image/png',
    buffer: redSquare,
  });
  await expect(page.getByText('Logo guardado.')).toBeVisible();

  await page.goto('/');
  await expect(page.getByRole('img', {name: /Logo de/})).toBeVisible();
});
