import {request, type ApiResult} from '#lib/core/http.ts';
import {toApiTime} from '#lib/core/time.ts';
import type {Hours} from '#lib/features/shop/types.ts';
import type {Barber, BarberInput} from './types.ts';

/** Trae todo el equipo para el panel. */
export function listAllBarbers(token: string | null): Promise<ApiResult<Barber[]>> {
  return request<Barber[]>('/api/barbers/all', {token});
}

/** Crea un barbero, con cuenta si se pide. */
export function createBarber(input: BarberInput, token: string | null): Promise<ApiResult<Barber>> {
  return request<Barber>('/api/barbers', {method: 'POST', body: input, token});
}

/** Edita el nombre, el estado y los servicios de un barbero. */
export function updateBarber(id: string, input: BarberInput, token: string | null): Promise<ApiResult<Barber>> {
  return request<Barber>(`/api/barbers/${id}`, {method: 'PUT', body: input, token});
}

/** Reemplaza el horario semanal de un barbero. */
export function setBarberSchedule(id: string, hours: Hours[], token: string | null): Promise<ApiResult<Barber>> {
  const body = {hours: hours.map((item) => ({...item, opens: toApiTime(item.opens), closes: toApiTime(item.closes)}))};
  return request<Barber>(`/api/barbers/${id}/schedule`, {method: 'PUT', body, token});
}

/** Registra una ausencia. */
export function addTimeOff(
  id: string,
  from: string,
  to: string,
  reason: string,
  token: string | null,
): Promise<ApiResult<Barber>> {
  return request<Barber>(`/api/barbers/${id}/time-off`, {method: 'POST', body: {from, to, reason}, token});
}

/** Quita una ausencia. */
export function removeTimeOff(id: string, timeOffId: string, token: string | null): Promise<ApiResult<Barber>> {
  return request<Barber>(`/api/barbers/${id}/time-off/${timeOffId}`, {method: 'DELETE', token});
}
