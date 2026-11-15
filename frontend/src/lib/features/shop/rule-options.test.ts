import {describe, expect, it} from 'vitest';
import {aheadOptions, cancelOptions, leadOptions, stepOptions} from './rule-options.ts';

describe('opciones de las reglas de reserva', () => {
  it('dice las anticipaciones en palabras', () => {
    const labels = leadOptions(60).map((option) => option.label);

    expect(labels).toContain('Se puede reservar al momento');
    expect(labels).toContain('Al menos 1 h antes');
    expect(labels).toContain('Al menos 1 día antes');
  });

  it('muestra los días como semanas o meses', () => {
    const labels = aheadOptions(14).map((option) => option.label);

    expect(labels).toEqual(expect.arrayContaining(['Hasta 1 semana adelante', 'Hasta 2 semanas adelante', 'Hasta 1 mes adelante']));
  });

  it('incluye un valor guardado que no está en la lista, en su lugar', () => {
    const options = cancelOptions(45);

    expect(options.map((option) => option.value)).toContain('45');
    expect(options.find((option) => option.value === '45')?.label).toBe('Hasta 45 min antes');
    expect(cancelOptions(2880).at(-1)?.label).toBe('Hasta 2 días antes');
  });

  it('ofrece los intervalos de turnos con ejemplos de horas', () => {
    const options = stepOptions();

    expect(options.map((option) => option.value)).toEqual(['15', '20', '30', '60']);
    expect(options[0].label).toBe('Cada 15 minutos (09:00, 09:15, 09:30…)');
    expect(options.at(-1)?.label).toBe('Cada hora (09:00, 10:00, 11:00…)');
  });
});
