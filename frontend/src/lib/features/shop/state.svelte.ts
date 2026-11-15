import {session} from '#lib/core/session.svelte.ts';
import {fromApiTime} from '#lib/core/time.ts';
import {shrinkImage} from '#lib/core/image.ts';
import {uploadImage} from '#lib/features/media/api.ts';
import {addClosedDay, getShop, removeClosedDay, setGallery, setLogo, setOpeningHours, updateShop} from './api.ts';
import type {Hours, Shop, ShopProfileInput} from './types.ts';

const logoSide = 512;
const photoSide = 1600;

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
    return this.run(updateShop(input, session.token), 'Cambios guardados.');
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

  /** Achica y sube el logo, y lo pone en la barbería. */
  async uploadLogo(file: File): Promise<void> {
    const id = await this.upload(file, logoSide, true);
    if (id) {
      await this.run(setLogo(id, session.token), 'Logo guardado.');
    }
  }

  /** Quita el logo. */
  removeLogo(): Promise<void> {
    return this.run(setLogo(null, session.token), 'Logo quitado.');
  }

  /** Achica y sube una foto del local, y la agrega al final. */
  async addPhoto(file: File): Promise<void> {
    const id = await this.upload(file, photoSide, false);
    if (id) {
      const ids = [...(this.shop?.galleryImageIds ?? []), id];
      await this.run(setGallery(ids, session.token), 'Foto agregada.');
    }
  }

  /** Quita una foto del local. */
  removePhoto(imageId: string): Promise<void> {
    const ids = (this.shop?.galleryImageIds ?? []).filter((id) => id !== imageId);
    return this.run(setGallery(ids, session.token), 'Foto quitada.');
  }

  /** Sube la imagen ya achicada y devuelve su id, o null si falló. */
  private async upload(file: File, maxSide: number, keepTransparency: boolean): Promise<string | null> {
    this.saving = true;
    try {
      const result = await uploadImage(await shrinkImage(file, maxSide, keepTransparency), session.token);
      if (!result.ok) {
        this.error = result.error.message;
        return null;
      }
      return result.data.id;
    }
    catch {
      this.error = 'No pudimos leer esa imagen. Prueba con otra foto.';
      return null;
    }
    finally {
      this.saving = false;
    }
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
