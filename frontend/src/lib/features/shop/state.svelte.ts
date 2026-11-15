import {session} from '#lib/core/session.svelte.ts';
import {fromApiTime} from '#lib/core/time.ts';
import {addClosedDay, getShop, removeClosedDay, setOpeningHours, updateShop} from './api.ts';
import type {Hours, Shop, ShopProfileInput} from './types.ts';

/** Estado de las páginas de horarios y ajustes de la barbería. */
export class ShopState {
  shop = $state<Shop | null>(null);
  loading = $state(true);
  saving = $state(false);
  error = $state<string | null>(null);
  notice = $state<string | null>(null);

  /** Horario de atención con las horas listas para editar. */
  get openingHours(): Hours[] {
    return (this.shop?.openingHours ?? []).map((hours) => ({
      ...hours,
      opens: fromApiTime(hours.opens),
      closes: fromApiTime(hours.closes),
    }));
  }

  /** Trae la barbería. */
  async load(): Promise<void> {
    this.loading = true;
    const result = await getShop();
    this.loading = false;
    if (result.ok) {
      this.shop = result.data;
      this.error = null;
    }
    else {
      this.error = result.error.message;
    }
  }

  /** Guarda los datos y las reglas. */
  saveProfile(input: ShopProfileInput): Promise<void> {
    return this.run(updateShop(input, session.token), 'Ajustes guardados.');
  }

  /** Guarda el horario semanal. */
  saveHours(hours: Hours[]): Promise<void> {
    return this.run(setOpeningHours(hours, session.token), 'Horario guardado.');
  }

  /** Marca una fecha como cerrada. */
  addClosedDay(date: string, reason: string): Promise<void> {
    return this.run(addClosedDay(date, reason, session.token), 'Día cerrado agregado.');
  }

  /** Quita una fecha cerrada. */
  removeClosedDay(date: string): Promise<void> {
    return this.run(removeClosedDay(date, session.token), 'Día cerrado quitado.');
  }

  /** Espera la llamada y actualiza la barbería o el error. */
  private async run(call: ReturnType<typeof getShop>, success: string): Promise<void> {
    this.saving = true;
    const result = await call;
    this.saving = false;
    if (result.ok) {
      this.shop = result.data;
      this.error = null;
      this.notice = success;
    }
    else {
      this.error = result.error.message;
      this.notice = null;
    }
  }
}
