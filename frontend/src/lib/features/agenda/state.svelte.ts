import type {ApiResult} from '#lib/core/http.ts';
import {session} from '#lib/core/session.svelte.ts';
import {addDays, todayInBolivia} from '#lib/core/time.ts';
import {listAllServices} from '#lib/features/services/api.ts';
import type {Service} from '#lib/features/services/types.ts';
import {addBlock, cancelStaffBooking, createStaffBooking, getAgenda, removeBlock, setBookingStatus} from './api.ts';
import type {Agenda, AgendaBlock, AgendaBooking, AgendaStatus, BlockInput, StaffBookingInput} from './types.ts';

/** Formulario abierto en la agenda. */
export type AgendaForm = 'none' | 'booking' | 'block';

/** Estado de la agenda del panel: el día, el turno elegido y los formularios. */
export class AgendaState {
  date = $state(todayInBolivia());
  agenda = $state<Agenda | null>(null);
  services = $state<Service[]>([]);
  selectedId = $state<string | null>(null);
  selectedBlockId = $state<string | null>(null);
  form = $state<AgendaForm>('none');
  loading = $state(true);
  busy = $state(false);
  error = $state<string | null>(null);

  /** Turno elegido, o el siguiente que falta atender hoy. */
  get focused(): AgendaBooking | null {
    const bookings = this.agenda?.bookings ?? [];
    const chosen = bookings.find((booking) => booking.id === this.selectedId);
    if (chosen) {
      return chosen;
    }
    const now = Date.now();
    return bookings.find((booking) => booking.status === 'Booked' && Date.parse(booking.end) > now) ?? null;
  }

  /** Bloqueo elegido, o null. */
  get selectedBlock(): AgendaBlock | null {
    return this.agenda?.blocks.find((block) => block.id === this.selectedBlockId) ?? null;
  }

  /** Nombre del barbero de un turno. */
  barberName(barberId: string): string {
    return this.agenda?.barbers.find((barber) => barber.id === barberId)?.name ?? '';
  }

  /** Cuántos turnos hay en cada estado. */
  get counts(): {total: number; attended: number; noShow: number} {
    const bookings = this.agenda?.bookings ?? [];
    return {
      total: bookings.length,
      attended: bookings.filter((booking) => booking.status === 'Attended').length,
      noShow: bookings.filter((booking) => booking.status === 'NoShow').length,
    };
  }

  /** Trae la agenda del día y los servicios para anotar turnos. */
  async load(): Promise<void> {
    this.loading = true;
    const [agenda, services] = await Promise.all([getAgenda(this.date, session.token), listAllServices(session.token)]);
    this.loading = false;
    if (!agenda.ok) {
      this.error = agenda.error.message;
      return;
    }
    this.agenda = agenda.data;
    if (services.ok) {
      this.services = services.data.filter((service) => service.isActive);
    }
    this.error = null;
  }

  /** Cambia de día: -1 para atrás, 1 para adelante, 0 para hoy. */
  move(days: number): Promise<void> {
    if (days === 0) {
      this.date = todayInBolivia();
    }
    else {
      this.date = addDays(this.date, days);
    }
    this.selectedId = null;
    this.selectedBlockId = null;
    return this.load();
  }

  /** Elige un turno para ver sus datos. */
  select(id: string): void {
    this.selectedBlockId = null;
    this.selectedId = id;
  }

  /** Elige un bloqueo para verlo o quitarlo, o null para cerrarlo. */
  selectBlock(id: string | null): void {
    this.selectedId = null;
    this.selectedBlockId = id;
  }

  /** Abre o cierra un formulario. */
  open(form: AgendaForm): void {
    this.form = form;
  }

  /** Marca cómo terminó el turno. */
  setStatus(id: string, status: AgendaStatus): Promise<void> {
    return this.run(setBookingStatus(id, status, session.token));
  }

  /** Cancela el turno. */
  cancel(id: string): Promise<void> {
    return this.run(cancelStaffBooking(id, session.token));
  }

  /** Anota un turno. */
  createBooking(input: StaffBookingInput): Promise<void> {
    return this.run(createStaffBooking(input, session.token));
  }

  /** Bloquea horas. */
  block(input: BlockInput): Promise<void> {
    return this.run(addBlock(input, session.token));
  }

  /** Quita un bloqueo y cierra su ficha. */
  async unblock(id: string): Promise<void> {
    await this.run(removeBlock(id, session.token));
    this.selectedBlockId = null;
  }

  /** Espera la llamada, cierra el formulario si salió bien y recarga el día. */
  private async run(call: Promise<ApiResult<unknown>>): Promise<void> {
    this.busy = true;
    const result = await call;
    this.busy = false;
    if (!result.ok) {
      this.error = result.error.message;
      return;
    }
    this.error = null;
    this.form = 'none';
    await this.load();
  }
}
