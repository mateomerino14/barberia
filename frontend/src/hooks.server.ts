import type {Handle} from '@sveltejs/kit/hooks';

/** Cabeceras de seguridad que el navegador respeta en todas las páginas. */
const securityHeaders: Record<string, string> = {
  'X-Content-Type-Options': 'nosniff',
  'X-Frame-Options': 'DENY',
  'Referrer-Policy': 'no-referrer',
  'Permissions-Policy': 'camera=(), microphone=(), geolocation=(), payment=()',
  'Cross-Origin-Opener-Policy': 'same-origin',
};

/** Agrega las cabeceras de seguridad a cada respuesta de la web. */
export const handle: Handle = async ({event, resolve}) => {
  const response = await resolve(event);
  for (const [name, value] of Object.entries(securityHeaders)) {
    response.headers.set(name, value);
  }
  return response;
};
