/** Día de la semana tal como lo envía la API. */
export type Weekday = 'sunday' | 'monday' | 'tuesday' | 'wednesday' | 'thursday' | 'friday' | 'saturday';

/** Días de la semana en el orden en que se leen en Bolivia, de lunes a domingo. */
export const weekdays: readonly Weekday[] = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday'];

const weekdayNames: Record<Weekday, string> = {
  monday: 'Lunes',
  tuesday: 'Martes',
  wednesday: 'Miércoles',
  thursday: 'Jueves',
  friday: 'Viernes',
  saturday: 'Sábado',
  sunday: 'Domingo',
};

const minutesPerHour = 60;
const hourMinuteLength = 5;
const secondsSuffix = ':00';

/** Devuelve el nombre en español de un día de la semana. */
export function weekdayName(day: Weekday): string {
  return weekdayNames[day];
}

/** Convierte "09:00:00" de la API en "09:00" para mostrar o editar. */
export function fromApiTime(value: string): string {
  return value.slice(0, hourMinuteLength);
}

/** Convierte "09:00" de un campo de hora en "09:00:00" para la API. */
export function toApiTime(value: string): string {
  return fromApiTime(value) + secondsSuffix;
}

/** Muestra una duración como "45 min" o "1 h 30 min". */
export function formatDuration(minutes: number): string {
  const hours = Math.floor(minutes / minutesPerHour);
  const rest = minutes % minutesPerHour;
  if (hours === 0) {
    return `${rest} min`;
  }
  if (rest === 0) {
    return `${hours} h`;
  }
  return `${hours} h ${rest} min`;
}

const dateFormatter = new Intl.DateTimeFormat('es-BO', {weekday: 'short', day: 'numeric', month: 'short', year: 'numeric', timeZone: 'UTC'});

/** Muestra una fecha "2026-12-25" como "vie, 25 dic 2026". */
export function formatDate(isoDate: string): string {
  return dateFormatter.format(new Date(`${isoDate}T00:00:00Z`));
}

/** Muestra un rango de fechas, o una sola si empieza y termina el mismo día. */
export function formatDateRange(from: string, to: string): string {
  if (from === to) {
    return formatDate(from);
  }
  return `${formatDate(from)} – ${formatDate(to)}`;
}
