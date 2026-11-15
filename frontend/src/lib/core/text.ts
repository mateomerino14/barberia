const maxInitials = 2;

/** Iniciales de un nombre, hasta dos, para un retrato sin foto. */
export function initials(name: string): string {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, maxInitials)
    .map((part) => part[0].toUpperCase())
    .join('');
}
