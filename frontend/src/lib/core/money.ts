const centsPerBoliviano = 100;
const centsDigits = 2;

/** Muestra un precio en centavos como "Bs 50" o "Bs 52,50". */
export function formatPrice(cents: number): string {
  const whole = Math.floor(cents / centsPerBoliviano);
  const rest = cents % centsPerBoliviano;
  if (rest === 0) {
    return `Bs ${whole}`;
  }
  return `Bs ${whole},${String(rest).padStart(centsDigits, '0')}`;
}

/** Convierte un precio escrito en bolivianos ("52,5" o "52.50") a centavos, o null si no es un número. */
export function parsePrice(text: string): number | null {
  const normalized = text.trim().replace(',', '.');
  if (normalized === '' || Number.isNaN(Number(normalized))) {
    return null;
  }
  return Math.round(Number(normalized) * centsPerBoliviano);
}

/** Convierte centavos en el texto que se edita en un campo de precio. */
export function priceInput(cents: number): string {
  return formatPrice(cents).replace('Bs ', '');
}
