import type {CustomerInput} from './types.ts';

const storageKey = 'barberia.customer';

/** Lee los datos del cliente guardados en este teléfono, si los hay. */
export function recallCustomer(storage: Storage = localStorage): CustomerInput | null {
  try {
    const raw = storage.getItem(storageKey);
    if (!raw) {
      return null;
    }
    const saved = JSON.parse(raw) as Partial<CustomerInput>;
    return {name: saved.name ?? '', phone: saved.phone ?? '', email: saved.email ?? ''};
  }
  catch {
    return null;
  }
}

/** Guarda los datos del cliente en este teléfono para la próxima reserva. */
export function rememberCustomer(customer: CustomerInput, storage: Storage = localStorage): void {
  try {
    storage.setItem(storageKey, JSON.stringify(customer));
  }
  catch {
    // Sin espacio o en modo privado: se reserva igual, solo no se recuerda.
  }
}

/** Borra los datos del cliente guardados en este teléfono. */
export function forgetCustomer(storage: Storage = localStorage): void {
  try {
    storage.removeItem(storageKey);
  }
  catch {
    // Nada que borrar si el almacenamiento no está disponible.
  }
}
