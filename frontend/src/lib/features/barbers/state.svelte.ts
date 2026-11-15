import {session} from '#lib/core/session.svelte.ts';
import {fromApiTime} from '#lib/core/time.ts';
import {listAllServices} from '#lib/features/services/api.ts';
import type {Service} from '#lib/features/services/types.ts';
import type {Hours} from '#lib/features/shop/types.ts';
import {
  addTimeOff,
  createBarber,
  listAllBarbers,
  listBarberAccess,
  removeTimeOff,
  setBarberAccess,
  setBarberSchedule,
  updateBarber,
} from './api.ts';
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
  access = $state<Record<string, string>>({});

  /** Barbero abierto, o null. */
  get selected(): Barber | null {
    return this.barbers.find((barber) => barber.id === this.selectedId) ?? null;
  }

  /** Correo con que entra al panel el barbero abierto, o null si no tiene acceso. */
  get selectedEmail(): string | null {
    if (!this.selectedId) {
      return null;
    }
    return this.access[this.selectedId] ?? null;
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
    const [barbers, services, access] = await Promise.all([
      listAllBarbers(session.token),
      listAllServices(session.token),
      listBarberAccess(session.token),
    ]);
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
    if (access.ok) {
      this.access = Object.fromEntries(access.data.map((item) => [item.barberId, item.email]));
    }
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
    if (input.account) {
      this.access = {...this.access, [result.data.id]: input.account.email.trim().toLowerCase()};
    }
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

  /** Da acceso al panel al barbero abierto o le pone una contraseña nueva. */
  async saveAccess(email: string | null, password: string): Promise<boolean> {
    const id = this.selectedId;
    if (!id) {
      return false;
    }
    this.saving = true;
    const result = await setBarberAccess(id, email, password, session.token);
    this.saving = false;
    if (!result.ok) {
      this.error = result.error.message;
      return false;
    }
    const hadAccess = Boolean(this.access[id]);
    this.access = {...this.access, [id]: result.data.email};
    if (hadAccess) {
      this.succeed('Contraseña cambiada. Su sesión abierta se cerró.');
    }
    else {
      this.succeed('Ya puede entrar al panel.');
    }
    return true;
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
