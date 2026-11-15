/** Enlace de Google Maps para llegar a una dirección. */
export function directionsHref(address: string): string {
  return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`;
}
