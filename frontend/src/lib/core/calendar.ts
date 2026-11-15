const lineBreak = '\r\n';

/** Datos de un evento para el calendario del teléfono. */
export interface CalendarEvent {
  id: string;
  title: string;
  location: string;
  start: string;
  end: string;
}

/** Convierte un instante ISO al formato UTC de los archivos de calendario ("20261114T143000Z"). */
function toIcsTime(iso: string): string {
  return new Date(iso).toISOString().replace(/[-:]/g, '').replace(/\.\d{3}/, '');
}

/** Arma el contenido de un archivo .ics con un solo evento. */
export function buildIcs(event: CalendarEvent): string {
  return [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//Barberia//Reservas//ES',
    'BEGIN:VEVENT',
    `UID:${event.id}@barberia`,
    `DTSTAMP:${toIcsTime(new Date().toISOString())}`,
    `DTSTART:${toIcsTime(event.start)}`,
    `DTEND:${toIcsTime(event.end)}`,
    `SUMMARY:${event.title}`,
    `LOCATION:${event.location}`,
    'END:VEVENT',
    'END:VCALENDAR',
  ].join(lineBreak);
}

/** Devuelve un enlace de descarga para el archivo .ics. */
export function icsHref(event: CalendarEvent): string {
  return `data:text/calendar;charset=utf-8,${encodeURIComponent(buildIcs(event))}`;
}
