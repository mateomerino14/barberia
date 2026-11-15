import {request, type ApiResult} from '#lib/core/http.ts';
import type {Service, ServiceInput} from './types.ts';

/** Trae todos los servicios para el panel. */
export function listAllServices(token: string | null): Promise<ApiResult<Service[]>> {
  return request<Service[]>('/api/services/all', {token});
}

/** Crea un servicio. */
export function createService(input: ServiceInput, token: string | null): Promise<ApiResult<Service>> {
  return request<Service>('/api/services', {method: 'POST', body: input, token});
}

/** Edita un servicio. */
export function updateService(id: string, input: ServiceInput, token: string | null): Promise<ApiResult<Service>> {
  return request<Service>(`/api/services/${id}`, {method: 'PUT', body: input, token});
}
