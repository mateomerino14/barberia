import {expect, test, type Page} from '@playwright/test';

const maxDaysToTry = 10;

/** Elige el primer día que tenga horas libres y devuelve si encontró uno. */
async function chooseFirstDayWithSlots(page: Page): Promise<boolean> {
  const days = page.locator('.day:not([disabled])');
  const count = Math.min(await days.count(), maxDaysToTry);
  for (let index = 0; index < count; index++) {
    await days.nth(index).click();
    const slot = page.locator('.slot').first();
    const empty = page.getByText(/No quedan horas libres/);
    await expect(slot.or(empty)).toBeVisible();
    if (await slot.isVisible()) {
      return true;
    }
  }
  return false;
}

test('la presentación muestra la barbería y lleva a reservar', async ({page}) => {
  await page.goto('/');
  await expect(page.getByRole('heading', {level: 1})).toBeVisible();
  await expect(page.getByText('Servicios y precios')).toBeVisible();
  await page.getByRole('link', {name: 'Reservar turno'}).click();
  await expect(page).toHaveURL(/\/reservar$/);
});

test('un cliente reserva desde el celular, ve su ticket y lo cancela', async ({page}) => {
  await page.goto('/reservar');
  await expect(page.getByRole('heading', {level: 1})).toBeVisible();

  await page.getByRole('radio', {name: /Degradado/}).first().check();
  await page.getByRole('button', {name: /Cualquiera/}).click();
  expect(await chooseFirstDayWithSlots(page)).toBe(true);
  await page.locator('.slot').first().click();
  await page.getByLabel('Nombre').fill('Diego');
  await page.getByLabel('Celular').fill('7 123 4567');
  await page.getByRole('button', {name: 'Reservar turno'}).click();

  await expect(page).toHaveURL(/\/turno\//);
  await expect(page.getByText('TURNO N.º')).toBeVisible();
  await expect(page.getByRole('heading', {name: /Te esperamos/})).toBeVisible();

  await page.getByRole('button', {name: 'Cancelar turno'}).click();
  await page.getByRole('button', {name: 'Sí, cancelar'}).click();
  await expect(page.getByRole('heading', {name: 'Turno cancelado'})).toBeVisible();
});

test('un enlace de turno que no existe lo dice claramente', async ({page}) => {
  await page.goto('/turno/no-existe');

  await expect(page.getByRole('heading', {name: 'No encontramos ese turno'})).toBeVisible();
});
