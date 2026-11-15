/** Hora libre con los barberos que pueden atender. */
export interface Slot {
  start: string;
  barberIds: string[];
}

/** Servicio que el cliente puede reservar. */
export interface PublicService {
  id: string;
  name: string;
  durationMinutes: number;
  priceCents: number;
}

/** Barbero que el cliente puede elegir. */
export interface PublicBarber {
  id: string;
  name: string;
  serviceIds: string[];
}

/** Datos que deja el cliente. */
export interface CustomerInput {
  name: string;
  phone: string;
  email: string;
}

/** Turno recién reservado. */
export interface BookingConfirmation {
  number: number;
  token: string;
  barberId: string;
  serviceName: string;
  priceCents: number;
  start: string;
  end: string;
}

/** Estado de un turno. */
type BookingStatus = 'Booked' | 'Attended' | 'NoShow' | 'Cancelled';

/** Turno como lo ve el cliente en su enlace. */
export interface CustomerBooking {
  number: number;
  status: BookingStatus;
  serviceId: string;
  serviceName: string;
  priceCents: number;
  barberId: string;
  barberName: string;
  customerName: string;
  start: string;
  end: string;
  changeUntil: string;
}
