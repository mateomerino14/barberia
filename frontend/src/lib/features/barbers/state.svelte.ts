import {session} from '#lib/core/session.svelte.ts';
import {fromApiTime} from '#lib/core/time.ts';
import {listAllServices} from '#lib/features/services/api.ts';
import type {Service} from '#lib/features/services/types.ts';
import type {Hours} from '#lib/features/shop/types.ts';
import {addTimeOff, createBarber, listAllBarbers, removeTimeOff, setBarberSchedule, updateBarber} from './api.ts';
import type {Barber, BarberInput} from './types.ts';

/** Estado de la página de barberos del panel. */
export class BarbersState {
  barbers = $state<Barber[]>([]);
  services = $state<Service[]>([]);
  loading = $state(true);
  saving = $state(false);
  error = $state<string | null>(null);
  notice = $state<string | null>(null);
  selectedId = $state<string | null>(null);
  creating = $state(false);

  /** Barbero abierto, o null. */
  get selected(): Barber | null {
    return this.barbers.find((barber) => barber.id === this.selectedId) ?? null;
  }

  /** Horario del barbero abierto, con las horas listas para editar. */
  get selectedSchedule(): Hours[] {
    return (this.selected?.schedule ?? []).map((hours) => ({
      ...hours,
      opens: fromApiTime(hours.opens),
      closes: fromApiTime(hours.closes),
    }));
  }

  /** Trae el equipo y los servicios. */
  async load(): Promise<void> {
    this.loading = true;
    const [barbers, services] = await Promise.all([listAllBarbers(session.token), listAllServices(session.token)]);
    this.loading = false;
    if (!barbers.ok) {
      this.error = barbers.error.message;
      return;
    }
    if (!services.ok) {
      this.error = services.error.message;
      return;
    }
    this.barbers = barbers.data;
    this.services = services.data;
    this.error = null;
  }

  /** Crea un barbero o guarda los cambios del abierto. */
  async save(input: BarberInput): Promise<void> {
    this.saving = true;
    let result;
    if (this.selectedId) {
      result = await updateBarber(this.selectedId, input, session.token);
    }
    else {
      result = await createBarber(input, session.token);
    }
    this.saving = false;
    if (!result.ok) {
      this.error = result.error.message;
      return;
    }
    this.replace(result.data);
    this.creating = false;
    this.selectedId = result.data.id;
    this.succeed('Barbero guardado.');
  }

  /** Guarda el horario del barbero abierto. */
  async saveSchedule(hours: Hours[]): Promise<void> {
    if (!this.selectedId) {
      return;
    }
    this.saving = true;
    const result = await setBarberSchedule(this.selectedId, hours, session.token);
    this.saving = false;
    if (!result.ok) {
      this.error = result.error.message;
      return;
    }
    this.replace(result.data);
    this.succeed('Horario guardado.');
  }

  /** Registra una ausencia del barbero abierto. */
  async addAbsence(from: string, to: string, reason: string): Promise<void> {
    if (!this.selectedId) {
      return;
    }
    this.saving = true;
    const result = await addTimeOff(this.selectedId, from, to, reason, session.token);
    this.saving = false;
    if (!result.ok) {
      this.error = result.error.message;
      return;
    }
    this.replace(result.data);
    this.succeed('Ausencia registrada.');
  }

  /** Quita una ausencia del barbero abierto. */
  async removeAbsence(timeOffId: string): Promise<void> {
    if (!this.selectedId) {
      return;
    }
    this.saving = true;
    const result = await removeTimeOff(this.selectedId, timeOffId, session.token);
    this.saving = false;
    if (!result.ok) {
      this.error = result.error.message;
      return;
    }
    this.replace(result.data);
    this.succeed('Ausencia quitada.');
  }

  /** Abre el formulario para un barbero nuevo. */
  startCreate(): void {
    this.selectedId = null;
    this.creating = true;
    this.notice = null;
  }

  /** Abre la ficha de un barbero. */
  open(id: string): void {
    this.creating = false;
    this.selectedId = id;
    this.notice = null;
  }

  /** Cierra la ficha o el formulario. */
  close(): void {
    this.creating = false;
    this.selectedId = null;
  }

  /** Reemplaza o agrega un barbero en la lista. */
  private replace(barber: Barber): void {
    const others = this.barbers.filter((item) => item.id !== barber.id);
    this.barbers = [...others, barber].sort((left, right) => left.name.localeCompare(right.name));
  }

  /** Limpia el error y muestra una confirmación. */
  private succeed(message: string): void {
    this.error = null;
    this.notice = message;
  }
}
