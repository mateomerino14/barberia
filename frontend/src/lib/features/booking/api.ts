import {request, type ApiResult} from '#lib/core/http.ts';
import type {
  BookingConfirmation,
  CustomerBooking,
  CustomerInput,
  DaySummary,
  PublicBarber,
  PublicService,
  Slot,
} from './types.ts';

/** Trae los servicios que se pueden reservar. */
export function listServices(): Promise<ApiResult<PublicService[]>> {
  return request<PublicService[]>('/api/services');
}

/** Trae los barberos que se pueden elegir. */
export function listBarbers(): Promise<ApiResult<PublicBarber[]>> {
  return request<PublicBarber[]>('/api/barbers');
}

/** Trae las horas libres de un día para un servicio, con un barbero o con cualquiera. */
export function getAvailability(date: string, serviceId: string, barberId: string | null): Promise<ApiResult<Slot[]>> {
  const params = new URLSearchParams({date, serviceId});
  if (barberId) {
    params.set('barberId', barberId);
  }
  return request<Slot[]>(`/api/availability?${params}`);
}

/** Resume los días que se pueden reservar, con o sin servicio y barbero elegidos. */
export function getDaySummary(serviceId: string | null, barberId: string | null): Promise<ApiResult<DaySummary[]>> {
  const params = new URLSearchParams();
  if (serviceId) {
    params.set('serviceId', serviceId);
  }
  if (barberId) {
    params.set('barberId', barberId);
  }
  return request<DaySummary[]>(`/api/availability/days?${params}`);
}

/** Reserva un turno. */
export function createBooking(
  serviceId: string,
  barberId: string | null,
  start: string,
  customer: CustomerInput,
): Promise<ApiResult<BookingConfirmation>> {
  const body = {
    serviceId,
    barberId,
    start,
    customerName: customer.name,
    customerPhone: customer.phone,
    customerEmail: customer.email,
  };
  return request<BookingConfirmation>('/api/bookings', {method: 'POST', body});
}

/** Trae un turno con la clave de su enlace. */
export function getBooking(token: string): Promise<ApiResult<CustomerBooking>> {
  return request<CustomerBooking>(`/api/bookings/${token}`);
}

/** Cancela un turno con la clave de su enlace. */
export function cancelBooking(token: string): Promise<ApiResult<CustomerBooking>> {
  return request<CustomerBooking>(`/api/bookings/${token}/cancel`, {method: 'POST'});
}

/** Mueve un turno a otra hora con la clave de su enlace. */
export function rescheduleBooking(token: string, start: string): Promise<ApiResult<CustomerBooking>> {
  return request<CustomerBooking>(`/api/bookings/${token}`, {method: 'PUT', body: {start}});
}
