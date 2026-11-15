import {request, type ApiResult} from '#lib/core/http.ts';
import {toApiTime} from '#lib/core/time.ts';
import type {Agenda, AgendaBlock, AgendaBooking, AgendaStatus, BlockInput, StaffBookingInput} from './types.ts';

/** Trae la agenda de un día. */
export function getAgenda(date: string, token: string | null): Promise<ApiResult<Agenda>> {
  return request<Agenda>(`/api/agenda?date=${date}`, {token});
}

/** Anota un turno desde el panel. */
export function createStaffBooking(input: StaffBookingInput, token: string | null): Promise<ApiResult<AgendaBooking>> {
  return request<AgendaBooking>('/api/agenda/bookings', {method: 'POST', body: input, token});
}

/** Marca cómo terminó un turno. */
export function setBookingStatus(
  id: string,
  status: AgendaStatus,
  token: string | null,
): Promise<ApiResult<AgendaBooking>> {
  return request<AgendaBooking>(`/api/agenda/bookings/${id}/status`, {method: 'PUT', body: {status}, token});
}

/** Cancela un turno desde el panel. */
export function cancelStaffBooking(id: string, token: string | null): Promise<ApiResult<AgendaBooking>> {
  return request<AgendaBooking>(`/api/agenda/bookings/${id}/cancel`, {method: 'POST', token});
}

/** Bloquea horas de un barbero. */
export function addBlock(input: BlockInput, token: string | null): Promise<ApiResult<AgendaBlock>> {
  const body = {...input, from: toApiTime(input.from), to: toApiTime(input.to)};
  return request<AgendaBlock>('/api/agenda/blocks', {method: 'POST', body, token});
}

/** Quita un bloqueo. */
export function removeBlock(id: string, token: string | null): Promise<ApiResult<undefined>> {
  return request<undefined>(`/api/agenda/blocks/${id}`, {method: 'DELETE', token});
}
