import {request, type ApiResult} from '#lib/core/http.ts';
import {toApiTime} from '#lib/core/time.ts';
import type {Hours, Shop, ShopProfileInput} from './types.ts';

/** Trae los datos públicos de la barbería. */
export function getShop(fetcher?: typeof fetch): Promise<ApiResult<Shop>> {
  return request<Shop>('/api/shop', {fetcher});
}

/** Guarda los datos y las reglas de la barbería. */
export function updateShop(input: ShopProfileInput, token: string | null): Promise<ApiResult<Shop>> {
  return request<Shop>('/api/shop', {method: 'PUT', body: input, token});
}

/** Reemplaza el horario semanal de atención. */
export function setOpeningHours(hours: Hours[], token: string | null): Promise<ApiResult<Shop>> {
  const body = {hours: hours.map((item) => ({...item, opens: toApiTime(item.opens), closes: toApiTime(item.closes)}))};
  return request<Shop>('/api/shop/hours', {method: 'PUT', body, token});
}

/** Marca una fecha como cerrada. */
export function addClosedDay(date: string, reason: string, token: string | null): Promise<ApiResult<Shop>> {
  return request<Shop>('/api/shop/closed-days', {method: 'POST', body: {date, reason}, token});
}

/** Quita una fecha cerrada. */
export function removeClosedDay(date: string, token: string | null): Promise<ApiResult<Shop>> {
  return request<Shop>(`/api/shop/closed-days/${date}`, {method: 'DELETE', token});
}
