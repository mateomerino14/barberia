import {fromApiTime, toMinutes, weekdayOf, weekdays, type Weekday} from '#lib/core/time.ts';
import type {Hours, Shop} from '#lib/features/shop/types.ts';

/** Si la barbería está abierta ahora y qué decir al respecto. */
export interface OpenStatus {
  open: boolean;
  text: string;
}

/** Fila del horario semanal para mostrar en la presentación. */
export interface ScheduleRow {
  day: Weekday;
  hours: Hours | null;
  today: boolean;
}

/** Dice si la barbería está abierta en la fecha y hora dadas, y hasta cuándo. */
export function openStatus(shop: Shop, today: string, clock: string): OpenStatus {
  const closedToday = shop.closedDays.some((day) => day.date === today);
  const hours = shop.openingHours.find((item) => item.day === weekdayOf(today));
  if (closedToday || !hours) {
    return {open: false, text: 'Cerrado hoy'};
  }
  const now = toMinutes(clock);
  const opens = fromApiTime(hours.opens);
  const closes = fromApiTime(hours.closes);
  if (now < toMinutes(opens)) {
    return {open: false, text: `Abre hoy a las ${opens}`};
  }
  if (now >= toMinutes(closes)) {
    return {open: false, text: 'Cerrado por hoy'};
  }
  return {open: true, text: `Abierto · cierra ${closes}`};
}

/** Arma el horario de lunes a domingo, marcando el día de hoy. */
export function weekSchedule(shop: Shop, today: string): ScheduleRow[] {
  const todayName = weekdayOf(today);
  return weekdays.map((day) => ({
    day,
    hours: shop.openingHours.find((item) => item.day === day) ?? null,
    today: day === todayName,
  }));
}
