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

/** Indica si esta barbería puede mandar el enlace para recuperar la contraseña. */
export async function passwordResetAvailable(): Promise<boolean> {
  const result = await request<{available: boolean}>('/api/auth/password-reset');
  return result.ok && result.data.available;
}

/** Pide el enlace para crear una contraseña nueva; la respuesta es la misma exista o no la cuenta. */
export function requestPasswordReset(email: string): Promise<ApiResult<undefined>> {
  return request<undefined>('/api/auth/password-reset', {method: 'POST', body: {email}});
}

/** Pone la contraseña nueva con la clave del enlace que llegó por correo. */
export function confirmPasswordReset(token: string, password: string): Promise<ApiResult<undefined>> {
  return request<undefined>('/api/auth/password-reset/confirm', {method: 'POST', body: {token, password}});
}
