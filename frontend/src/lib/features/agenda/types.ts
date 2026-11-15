/** Barbero en la agenda, con su horario del día en hora local o null si no trabaja. */
export interface AgendaBarber {
  id: string;
  name: string;
  opens: string | null;
  closes: string | null;
}

/** Estado de un turno en la agenda. */
export type AgendaStatus = 'Booked' | 'Attended' | 'NoShow';

/** Turno en la agenda del panel. */
export interface AgendaBooking {
  id: string;
  number: number;
  barberId: string;
  serviceId: string;
  serviceName: string;
  priceCents: number;
  start: string;
  end: string;
  status: AgendaStatus;
  customerName: string;
  customerPhone: string;
  isWalkIn: boolean;
}

/** Horas bloqueadas de un barbero. */
export interface AgendaBlock {
  id: string;
  barberId: string;
  start: string;
  end: string;
  reason: string;
}

/** Agenda de un día. */
export interface Agenda {
  date: string;
  barbers: AgendaBarber[];
  bookings: AgendaBooking[];
  blocks: AgendaBlock[];
}

/** Turno que anota la barbería. */
export interface StaffBookingInput {
  serviceId: string;
  barberId: string;
  start: string;
  customerName: string;
  customerPhone: string;
  isWalkIn: boolean;
}

/** Bloqueo nuevo. */
export interface BlockInput {
  barberId: string;
  date: string;
  from: string;
  to: string;
  reason: string;
}
