import {addDays, todayInBolivia, weekdayOf} from '#lib/core/time.ts';
import {getShop} from '#lib/features/shop/api.ts';
import type {Shop} from '#lib/features/shop/types.ts';
import {forgetCustomer, recallCustomer, rememberCustomer} from './customer-memory.ts';
import {createBooking, getAvailability, getDaySummary, listBarbers, listServices} from './api.ts';
import type {BookingConfirmation, CustomerInput, PublicBarber, PublicService, Slot} from './types.ts';

/** Día que se puede elegir en la tira de fechas; sin resumen todavía, las horas libres son null. */
export interface DayOption {
  date: string;
  open: boolean;
  freeStarts: number | null;
}

/** Estado del flujo de reserva del cliente: servicio, barbero, día, hora y datos. */
export class BookingFlow {
  shop = $state<Shop | null>(null);
  services = $state<PublicService[]>([]);
  barbers = $state<PublicBarber[]>([]);
  serviceId = $state<string | null>(null);
  barberId = $state<string | null>(null);
  date = $state(todayInBolivia());
  slots = $state<Slot[]>([]);
  freeByDate = $state<Record<string, number> | null>(null);
  start = $state<string | null>(null);
  customer = $state<CustomerInput>({name: '', phone: '', email: ''});
  remember = $state(true);
  loading = $state(true);
  loadingSlots = $state(false);
  sending = $state(false);
  error = $state<string | null>(null);

  /** Servicio elegido. */
  get service(): PublicService | null {
    return this.services.find((item) => item.id === this.serviceId) ?? null;
  }

  /** Barberos que hacen el servicio elegido. */
  get availableBarbers(): PublicBarber[] {
    const serviceId = this.serviceId;
    if (!serviceId) {
      return this.barbers;
    }
    return this.barbers.filter((barber) => barber.serviceIds.includes(serviceId));
  }

  /** Nombre del barbero elegido o "cualquiera". */
  get barberName(): string {
    return this.barbers.find((barber) => barber.id === this.barberId)?.name ?? 'cualquiera';
  }

  /** Días que se pueden reservar, marcando los que la barbería no abre. */
  get days(): DayOption[] {
    const today = todayInBolivia();
    const shop = this.shop;
    if (!shop) {
      return [];
    }
    const openDays = shop.openingHours.map((hours) => hours.day);
    const closedDates = shop.closedDays.map((day) => day.date);
    const options: DayOption[] = [];
    for (let offset = 0; offset <= shop.rules.maxDaysAhead; offset++) {
      const date = addDays(today, offset);
      let freeStarts: number | null = null;
      if (this.freeByDate) {
        freeStarts = this.freeByDate[date] ?? 0;
      }
      options.push({date, open: openDays.includes(weekdayOf(date)) && !closedDates.includes(date), freeStarts});
    }
    return options;
  }

  /** Indica si ya se puede enviar la reserva. */
  get ready(): boolean {
    return Boolean(this.serviceId && this.start && this.customer.name.trim() && this.customer.phone.trim());
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
    const saved = recallCustomer();
    if (saved) {
      this.customer = saved;
    }
    await this.loadDays();
  }

  /** Elige el servicio, actualiza los días llenos y vuelve a buscar horas. */
  async chooseService(serviceId: string): Promise<void> {
    this.serviceId = serviceId;
    if (this.barberId && !this.availableBarbers.some((barber) => barber.id === this.barberId)) {
      this.barberId = null;
    }
    await this.loadDays();
    await this.loadSlots();
  }

  /** Elige un barbero, o null para cualquiera, actualiza los días llenos y vuelve a buscar horas. */
  async chooseBarber(barberId: string | null): Promise<void> {
    this.barberId = barberId;
    await this.loadDays();
    await this.loadSlots();
  }

  /** Trae cuántas horas libres le quedan a cada día y, si el elegido no tiene lugar, pasa al primero que sí. */
  async loadDays(): Promise<void> {
    const result = await getDaySummary(this.serviceId, this.barberId);
    this.freeByDate = null;
    if (result.ok) {
      this.freeByDate = Object.fromEntries(result.data.map((day) => [day.date, day.freeStarts]));
    }
    const current = this.days.find((day) => day.date === this.date);
    if (!current || !hasRoom(current)) {
      this.date = this.days.find(hasRoom)?.date ?? this.date;
    }
  }

  /** Elige el día y vuelve a buscar horas. */
  chooseDate(date: string): Promise<void> {
    this.date = date;
    return this.loadSlots();
  }

  /** Elige la hora. */
  chooseStart(start: string): void {
    this.start = start;
  }

  /** Busca las horas libres para lo elegido. */
  async loadSlots(): Promise<void> {
    this.start = null;
    const serviceId = this.serviceId;
    if (!serviceId) {
      this.slots = [];
      return;
    }
    this.loadingSlots = true;
    const result = await getAvailability(this.date, serviceId, this.barberId);
    this.loadingSlots = false;
    if (result.ok) {
      this.slots = result.data;
      this.error = null;
    }
    else {
      this.slots = [];
      this.error = result.error.message;
    }
  }

  /** Envía la reserva; si la hora se ocupó, vuelve a buscar horas. */
  async submit(): Promise<BookingConfirmation | null> {
    const serviceId = this.serviceId;
    const start = this.start;
    if (!serviceId || !start) {
      return null;
    }
    this.sending = true;
    const result = await createBooking(serviceId, this.barberId, start, this.customer);
    this.sending = false;
    if (result.ok) {
      this.keepCustomer();
      return result.data;
    }
    this.error = result.error.message;
    if (result.error.code === 'booking.slot_unavailable') {
      await this.loadSlots();
      this.error = result.error.message;
    }
    return null;
  }

  /** Guarda o borra los datos del cliente según lo que eligió. */
  private keepCustomer(): void {
    if (this.remember) {
      rememberCustomer(this.customer);
    }
    else {
      forgetCustomer();
    }
  }
}

/** Indica si un día abre y no está lleno; sin resumen, basta con que abra. */
function hasRoom(day: DayOption): boolean {
  return day.open && day.freeStarts !== 0;
}
