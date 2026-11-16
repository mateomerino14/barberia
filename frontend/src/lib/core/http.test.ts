import {describe, expect, it, vi} from 'vitest';
import {onSessionEnded, request} from './http.ts';

/** Arma un fetch falso que responde siempre lo mismo. */
function fakeFetch(status: number, body: unknown) {
  return vi.fn(async () => new Response(JSON.stringify(body), {status, headers: {'Content-Type': 'application/json'}}));
}

describe('request', () => {
  it('devuelve los datos cuando la API responde bien y envía el pase', async () => {
    const fetcher = fakeFetch(200, {name: 'Don Lucho'});

    const result = await request<{name: string}>('/api/shop', {token: 'abc', fetcher});

    expect(result).toEqual({ok: true, data: {name: 'Don Lucho'}});
    const init = fetcher.mock.calls[0] as unknown as [string, RequestInit];
    expect((init[1].headers as Record<string, string>).Authorization).toBe('Bearer abc');
  });

  it('convierte un ProblemDetails en un error con mensaje y código', async () => {
    const fetcher = fakeFetch(409, {title: 'Ese día ya está marcado como cerrado.', code: 'shop.closed_day_exists'});

    const result = await request('/api/shop/closed-days', {method: 'POST', body: {}, fetcher});

    expect(result).toEqual({
      ok: false,
      error: {status: 409, code: 'shop.closed_day_exists', message: 'Ese día ya está marcado como cerrado.'},
    });
  });

  it('avisa cuando no hay conexión', async () => {
    const fetcher = vi.fn(async () => {
      throw new TypeError('Failed to fetch');
    });

    const result = await request('/api/shop', {fetcher});

    expect(result.ok).toBe(false);
  });

  it('avisa que la sesión terminó cuando la API rechaza el pase', async () => {
    const ended = vi.fn();
    onSessionEnded(ended);

    await request('/api/agenda', {token: 'vencido', fetcher: fakeFetch(401, {})});
    await request('/api/auth/login', {method: 'POST', body: {}, fetcher: fakeFetch(401, {})});
    onSessionEnded(null);

    expect(ended).toHaveBeenCalledTimes(1);
  });

  it('acepta respuestas sin contenido, como un 202', async () => {
    const fetcher = vi.fn(async () => new Response(null, {status: 202}));

    const result = await request('/api/auth/password-reset', {method: 'POST', body: {}, fetcher});

    expect(result).toEqual({ok: true, data: undefined});
  });
});
