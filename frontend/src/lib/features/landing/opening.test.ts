import {describe, expect, it} from 'vitest';
import type {Shop} from '#lib/features/shop/types.ts';
import {openStatus, weekSchedule} from './opening.ts';

const monday = '2026-11-16';
const tuesday = '2026-11-17';
const sunday = '2026-11-22';
const daysInWeek = 7;

const shop: Shop = {
  name: 'Barbería',
  address: 'Av. 20 de Octubre 2150, La Paz',
  phone: '70000000',
  welcomeMessage: '',
  rules: {minLeadMinutes: 60, maxDaysAhead: 14, cancelLimitMinutes: 120},
  palette: 'classic',
  openingHours: [
    {day: 'monday', opens: '09:00:00', closes: '20:00:00'},
    {day: 'tuesday', opens: '09:00:00', closes: '20:00:00'},
  ],
  closedDays: [{date: tuesday, reason: 'Feriado'}],
  logoImageId: null,
  galleryImageIds: [],
};

describe('openStatus', () => {
  it('dice que está abierta y a qué hora cierra', () => {
    expect(openStatus(shop, monday, '10:30')).toEqual({open: true, text: 'Abierto · cierra 20:00'});
  });

  it('dice a qué hora abre si es temprano', () => {
    expect(openStatus(shop, monday, '07:00')).toEqual({open: false, text: 'Abre hoy a las 09:00'});
  });

  it('dice que ya cerró pasada la hora', () => {
    expect(openStatus(shop, monday, '20:00').text).toBe('Cerrado por hoy');
  });

  it('está cerrada en fechas cerradas y días sin horario', () => {
    expect(openStatus(shop, tuesday, '10:00').text).toBe('Cerrado hoy');
    expect(openStatus(shop, sunday, '10:00').text).toBe('Cerrado hoy');
  });
});

describe('weekSchedule', () => {
  it('lista la semana desde el lunes y marca hoy', () => {
    const rows = weekSchedule(shop, monday);
    expect(rows).toHaveLength(daysInWeek);
    expect(rows[0]).toMatchObject({day: 'monday', today: true});
    expect(rows.at(-1)).toMatchObject({day: 'sunday', hours: null, today: false});
  });
});
