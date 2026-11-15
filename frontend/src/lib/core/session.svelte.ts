const storageKey = 'barberia.session';

/** Rol de la cuenta con sesión abierta. */
export type StaffRole = 'Owner' | 'Barber';

/** Sesión del panel guardada en el navegador. */
export interface SessionData {
  token: string;
  expiresAt: string;
  email: string;
  role: StaffRole;
  barberId: string | null;
}

/** Guarda la sesión del panel en memoria y en el navegador. */
class Session {
  current = $state<SessionData | null>(null);

  /** Carga la sesión guardada si todavía no venció. */
  load(): void {
    try {
      const saved = localStorage.getItem(storageKey);
      if (!saved) {
        return;
      }
      const data = JSON.parse(saved) as SessionData;
      if (new Date(data.expiresAt) > new Date()) {
        this.current = data;
      }
      else {
        localStorage.removeItem(storageKey);
      }
    }
    catch {
      this.current = null;
    }
  }

  /** Abre la sesión y la recuerda en el navegador. */
  open(data: SessionData): void {
    this.current = data;
    try {
      localStorage.setItem(storageKey, JSON.stringify(data));
    }
    catch {
      // Sin almacenamiento la sesión dura solo mientras la página esté abierta.
    }
  }

  /** Cierra la sesión. */
  close(): void {
    this.current = null;
    try {
      localStorage.removeItem(storageKey);
    }
    catch {
      // Nada que borrar si el navegador no deja guardar.
    }
  }

  /** Pase de la sesión actual, o null. */
  get token(): string | null {
    return this.current?.token ?? null;
  }

  /** Indica si la sesión es del dueño. */
  get isOwner(): boolean {
    return this.current?.role === 'Owner';
  }
}

export const session = new Session();
