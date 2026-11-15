import type {Hours} from '#lib/features/shop/types.ts';

/** Ausencia de un barbero. */
interface TimeOff {
  id: string;
  from: string;
  to: string;
  reason: string;
}

/** Barbero como lo ve el panel. */
export interface Barber {
  id: string;
  name: string;
  isActive: boolean;
  serviceIds: string[];
  schedule: Hours[];
  timeOff: TimeOff[];
}

/** Datos de un barbero que se crea o edita. */
export interface BarberInput {
  name: string;
  isActive: boolean;
  serviceIds: string[];
  account?: {email: string; password: string} | null;
}

/** Barbero con acceso al panel y el correo con que entra. */
export interface BarberAccess {
  barberId: string;
  email: string;
}
