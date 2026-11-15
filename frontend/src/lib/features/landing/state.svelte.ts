import {nowClockInBolivia, todayInBolivia} from '#lib/core/time.ts';
import {listBarbers, listServices} from '#lib/features/booking/api.ts';
import type {PublicBarber, PublicService} from '#lib/features/booking/types.ts';
import {getShop} from '#lib/features/shop/api.ts';
import type {Shop} from '#lib/features/shop/types.ts';
import {openStatus, weekSchedule, type OpenStatus, type ScheduleRow} from './opening.ts';

/** Datos de la página de presentación: la barbería, sus precios y su equipo. */
export class LandingState {
  shop = $state<Shop | null>(null);
  services = $state<PublicService[]>([]);
  barbers = $state<PublicBarber[]>([]);
  loading = $state(true);
  error = $state<string | null>(null);

  /** Si está abierta ahora, en hora de La Paz. */
  get status(): OpenStatus | null {
    if (!this.shop) {
      return null;
    }
    return openStatus(this.shop, todayInBolivia(), nowClockInBolivia());
  }

  /** Horario de la semana con hoy marcado. */
  get schedule(): ScheduleRow[] {
    if (!this.shop) {
      return [];
    }
    return weekSchedule(this.shop, todayInBolivia());
  }

  /** Trae la barbería, los servicios y los barberos. */
  async load(): Promise<void> {
    const [shop, services, barbers] = await Promise.all([getShop(), listServices(), listBarbers()]);
    this.loading = false;
    if (!shop.ok || !services.ok || !barbers.ok) {
      this.error = 'No pudimos cargar la barbería. Intenta de nuevo en un momento.';
      return;
    }
    this.shop = shop.data;
    this.services = services.data;
    this.barbers = barbers.data;
  }
}
