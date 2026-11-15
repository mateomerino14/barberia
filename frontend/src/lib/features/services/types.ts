/** Servicio del catálogo. */
export interface Service {
  id: string;
  name: string;
  durationMinutes: number;
  priceCents: number;
  isActive: boolean;
}

/** Datos de un servicio que se crea o edita. */
export interface ServiceInput {
  name: string;
  durationMinutes: number;
  priceCents: number;
  isActive: boolean;
}
