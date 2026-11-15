import {request, type ApiResult} from '#lib/core/http.ts';
import type {SessionData} from '#lib/core/session.svelte.ts';

/** Inicia sesión con correo y contraseña. */
export function login(email: string, password: string): Promise<ApiResult<SessionData>> {
  return request<SessionData>('/api/auth/login', {method: 'POST', body: {email, password}});
}

/** Cambia la contraseña propia; devuelve la sesión nueva porque las anteriores se cierran. */
export function changePassword(
  currentPassword: string,
  newPassword: string,
  token: string | null,
): Promise<ApiResult<SessionData>> {
  return request<SessionData>('/api/auth/password', {method: 'POST', body: {currentPassword, newPassword}, token});
}
