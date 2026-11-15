import {describe, expect, it} from 'vitest';
import {buildIcs} from './calendar.ts';
import {addDays, clockTime, dateOf, todayInBolivia, weekdayOf} from './time.ts';
import {whatsappHref} from './whatsapp.ts';

describe('fechas de la reserva', () => {
  it('usa la fecha de La Paz aunque en UTC ya sea el día siguiente', () => {
    expect(todayInBolivia(new Date('2026-11-15T02:00:00Z'))).toBe('2026-11-14');
  });

  it('suma días y reconoce el día de la semana', () => {
    expect(addDays('2026-11-30', 1)).toBe('2026-12-01');
    expect(weekdayOf('2026-11-14')).toBe('saturday');
  });

  it('lee la hora y la fecha que manda la API en hora local', () => {
    expect(clockTime('2026-11-15T10:45:00-04:00')).toBe('10:45');
    expect(dateOf('2026-11-15T10:45:00-04:00')).toBe('2026-11-15');
  });
});

describe('calendario', () => {
  it('arma un evento con las horas en UTC', () => {
    const ics = buildIcs({id: '1', title: 'Degradado', location: 'Sopocachi', start: '2026-11-15T10:45:00-04:00', end: '2026-11-15T11:30:00-04:00'});

    expect(ics).toContain('DTSTART:20261115T144500Z');
    expect(ics).toContain('DTEND:20261115T153000Z');
    expect(ics).toContain('SUMMARY:Degradado');
  });
});

describe('whatsapp', () => {
  it('agrega el código de Bolivia a un celular local y codifica el mensaje', () => {
    expect(whatsappHref('7 123 4567', 'Hola Diego')).toBe('https://wa.me/59171234567?text=Hola%20Diego');
    expect(whatsappHref('+591 71234567', 'x')).toBe('https://wa.me/59171234567?text=x');
  });
});
