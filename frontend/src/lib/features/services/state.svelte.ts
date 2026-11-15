import {session} from '#lib/core/session.svelte.ts';
import {createService, listAllServices, updateService} from './api.ts';
import type {Service, ServiceInput} from './types.ts';

/** Estado de la página de servicios del panel. */
export class ServicesState {
  services = $state<Service[]>([]);
  loading = $state(true);
  saving = $state(false);
  error = $state<string | null>(null);
  editingId = $state<string | null>(null);
  creating = $state(false);

  /** Servicio que se está editando, o null. */
  get editing(): Service | null {
    return this.services.find((service) => service.id === this.editingId) ?? null;
  }

  /** Trae todos los servicios. */
  async load(): Promise<void> {
    this.loading = true;
    const result = await listAllServices(session.token);
    this.loading = false;
    if (result.ok) {
      this.services = result.data;
      this.error = null;
    }
    else {
      this.error = result.error.message;
    }
  }

  /** Crea o edita según lo que esté abierto, y cierra el formulario si salió bien. */
  async save(input: ServiceInput): Promise<void> {
    this.saving = true;
    let result;
    if (this.editingId) {
      result = await updateService(this.editingId, input, session.token);
    }
    else {
      result = await createService(input, session.token);
    }
    this.saving = false;
    if (!result.ok) {
      this.error = result.error.message;
      return;
    }
    this.error = null;
    this.close();
    await this.load();
  }

  /** Abre el formulario para un servicio nuevo. */
  startCreate(): void {
    this.editingId = null;
    this.creating = true;
  }

  /** Abre el formulario para editar un servicio. */
  startEdit(id: string): void {
    this.creating = false;
    this.editingId = id;
  }

  /** Cierra el formulario. */
  close(): void {
    this.creating = false;
    this.editingId = null;
  }
}
