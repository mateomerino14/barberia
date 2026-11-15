const boliviaCode = '591';
const localNumberLength = 8;

/** Arma el enlace que abre WhatsApp con el número y el mensaje listos. */
export function whatsappHref(phone: string, message: string): string {
  let digits = phone.replace(/\D/g, '');
  if (digits.length === localNumberLength) {
    digits = boliviaCode + digits;
  }
  return `https://wa.me/${digits}?text=${encodeURIComponent(message)}`;
}
