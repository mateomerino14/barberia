import type {Weekday} from '#lib/core/time.ts';

/** Horario de un día. */
export interface Hours {
  day: Weekday;
  opens: string;
  closes: string;
}

/** Fecha en la que la barbería no abre. */
export interface ClosedDay {
  date: string;
  reason: string;
}

/** Reglas de reserva. */
export interface BookingRules {
  minLeadMinutes: number;
  maxDaysAhead: number;
  cancelLimitMinutes: number;
}

/** Datos de la barbería. */
export interface Shop {
  name: string;
  address: string;
  phone: string;
  welcomeMessage: string;
  rules: BookingRules;
  openingHours: Hours[];
  closedDays: ClosedDay[];
}

/** Datos que edita el dueño. */
export interface ShopProfileInput {
  name: string;
  address: string;
  phone: string;
  welcomeMessage: string;
  rules: BookingRules;
}
