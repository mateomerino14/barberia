import {formatDuration} from '#lib/core/time.ts';

/** Opción de una lista desplegable. */
interface RuleOption {
  value: string;
  label: string;
}

// Las tres listas son las opciones que ve el dueño; cada número es un valor de la lista, no un valor suelto.
/* eslint-disable @typescript-eslint/no-magic-numbers */
const leadMinutes = [0, 15, 30, 60, 120, 180, 360, 720, 1440];
const cancelMinutes = [0, 30, 60, 120, 180, 360, 720, 1440, 2880];
const aheadDays = [7, 14, 21, 30, 60, 90];
/* eslint-enable @typescript-eslint/no-magic-numbers */
const daysPerWeek = 7;
const daysPerMonth = 30;
const minutesPerDay = 1440;

/** Muestra minutos como texto corto: "1 h", "1 día", "2 días". */
function minutesLabel(minutes: number): string {
  if (minutes >= minutesPerDay && minutes % minutesPerDay === 0) {
    const days = minutes / minutesPerDay;
    if (days === 1) {
      return '1 día';
    }
    return `${days} días`;
  }
  return formatDuration(minutes);
}

/** Muestra días como semanas o meses cuando calzan justo. */
function daysLabel(days: number): string {
  if (days % daysPerMonth === 0) {
    const months = days / daysPerMonth;
    if (months === 1) {
      return '1 mes';
    }
    return `${months} meses`;
  }
  if (days % daysPerWeek === 0) {
    const weeks = days / daysPerWeek;
    if (weeks === 1) {
      return '1 semana';
    }
    return `${weeks} semanas`;
  }
  return `${days} días`;
}

/** Arma las opciones e incluye el valor guardado si no está en la lista. */
function withCurrent(values: number[], current: number, label: (value: number) => string): RuleOption[] {
  const all = [...values];
  if (!all.includes(current)) {
    all.push(current);
  }
  return all.sort((left, right) => left - right).map((value) => ({value: String(value), label: label(value)}));
}

/** Opciones de anticipación mínima para reservar. */
export function leadOptions(current: number): RuleOption[] {
  return withCurrent(leadMinutes, current, (value) => {
    if (value === 0) {
      return 'Se puede reservar al momento';
    }
    return `Al menos ${minutesLabel(value)} antes`;
  });
}

/** Opciones de cuántos días hacia adelante se puede reservar. */
export function aheadOptions(current: number): RuleOption[] {
  return withCurrent(aheadDays, current, (value) => `Hasta ${daysLabel(value)} adelante`);
}

/** Opciones de plazo para que el cliente cambie o cancele. */
export function cancelOptions(current: number): RuleOption[] {
  return withCurrent(cancelMinutes, current, (value) => {
    if (value === 0) {
      return 'Hasta la hora del turno';
    }
    return `Hasta ${minutesLabel(value)} antes`;
  });
}
