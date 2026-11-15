import {getShop} from '#lib/features/shop/api.ts';
import type {Shop} from '#lib/features/shop/types.ts';
import {cancelBooking, getAvailability, getBooking, rescheduleBooking} from './api.ts';
import type {CustomerBooking, Slot} from './types.ts';

/** Estado de la página del turno del cliente: verlo, cambiar la hora o cancelarlo. */
export class TicketState {
  booking = $state<CustomerBooking | null>(null);
  shop = $state<Shop | null>(null);
  loading = $state(true);
  busy = $state(false);
  error = $state<string | null>(null);
  notice = $state<string | null>(null);
  changing = $state(false);
  date = $state('');
  slots = $state<Slot[]>([]);

  /** Crea el estado para la clave del enlace. */
  constructor(private readonly token: string) {}

  /** Indica si el cliente todavía puede cambiar o cancelar por su cuenta. */
  get canChange(): boolean {
    const booking = this.booking;
    return booking !== null && booking.status === 'Booked' && Date.parse(booking.changeUntil) > Date.now();
  }

  /** Trae el turno y la barbería. */
  async load(): Promise<void> {
    const [booking, shop] = await Promise.all([getBooking(this.token), getShop()]);
    this.loading = false;
    if (!booking.ok) {
      this.error = booking.error.message;
      return;
    }
    this.booking = booking.data;
    if (shop.ok) {
      this.shop = shop.data;
    }
  }

  /** Cancela el turno. */
  async cancel(): Promise<void> {
    this.busy = true;
    const result = await cancelBooking(this.token);
    this.busy = false;
    this.apply(result, 'Tu turno quedó cancelado.');
  }

  /** Abre el cambio de hora en el mismo día del turno. */
  async startChange(date: string): Promise<void> {
    this.changing = true;
    await this.loadSlots(date);
  }

  /** Busca horas libres con el mismo barbero y servicio. */
  async loadSlots(date: string): Promise<void> {
    const booking = this.booking;
    if (!booking) {
      return;
    }
    this.date = date;
    const result = await getAvailability(date, booking.serviceId, booking.barberId);
    if (result.ok) {
      this.slots = result.data;
    }
    else {
      this.slots = [];
      this.error = result.error.message;
    }
  }

  /** Mueve el turno a la hora elegida. */
  async moveTo(start: string): Promise<void> {
    this.busy = true;
    const result = await rescheduleBooking(this.token, start);
    this.busy = false;
    this.apply(result, 'Listo, cambiamos la hora de tu turno.');
    if (result.ok) {
      this.changing = false;
    }
  }

  /** Actualiza el turno o muestra el error de la llamada. */
  private apply(result: Awaited<ReturnType<typeof getBooking>>, success: string): void {
    if (result.ok) {
      this.booking = result.data;
      this.error = null;
      this.notice = success;
    }
    else {
      this.error = result.error.message;
      this.notice = null;
    }
  }
}
