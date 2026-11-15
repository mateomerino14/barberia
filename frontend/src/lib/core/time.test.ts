import {describe, expect, it} from 'vitest';
import {formatDate, formatDateRange, formatDuration, fromApiTime, toApiTime, weekdayName} from './time.ts';

describe('time', () => {
  it('muestra duraciones en minutos y horas', () => {
    expect(formatDuration(45)).toBe('45 min');
    expect(formatDuration(60)).toBe('1 h');
    expect(formatDuration(90)).toBe('1 h 30 min');
  });

  it('convierte horas entre la API y los campos', () => {
    expect(fromApiTime('09:30:00')).toBe('09:30');
    expect(toApiTime('09:30')).toBe('09:30:00');
    expect(toApiTime('09:30:00')).toBe('09:30:00');
  });

  it('nombra los días en español', () => {
    expect(weekdayName('wednesday')).toBe('Miércoles');
  });

  it('muestra fechas sin correrse de día por la zona horaria', () => {
    expect(formatDate('2026-12-25')).toContain('25');
    expect(formatDateRange('2026-12-01', '2026-12-01')).toBe(formatDate('2026-12-01'));
    expect(formatDateRange('2026-12-01', '2026-12-05')).toContain('–');
  });
});
