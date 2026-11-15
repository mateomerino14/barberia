import {request, type ApiResult} from '#lib/core/http.ts';
import type {SessionData} from '#lib/core/session.svelte.ts';

/** Inicia sesión con correo y contraseña. */
export function login(email: string, password: string): Promise<ApiResult<SessionData>> {
  return request<SessionData>('/api/auth/login', {method: 'POST', body: {email, password}});
}
