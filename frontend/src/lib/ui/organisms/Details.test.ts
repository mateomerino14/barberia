import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import BlockDetail from './BlockDetail.svelte';
import BookingDetail from './BookingDetail.svelte';
import Ticket from './Ticket.svelte';

const start = '2026-11-17T10:30:00-04:00';
const detail = {
  number: 12,
  customerName: 'Diego',
  customerPhone: '71234567',
  serviceName: 'Degradado',
  barberName: 'Lucho',
  start,
  priceCents: 5000,
  status: 'Booked' as const,
  isWalkIn: false,
};

/** Acciones vacías del detalle de un turno. */
function actions() {
  return {onAttended: vi.fn(), onNoShow: vi.fn(), onUndo: vi.fn(), onCancel: vi.fn()};
}

describe('BookingDetail', () => {
  it('un turno reservado se puede marcar como atendido', async () => {
    const handlers = actions();
    render(BookingDetail, {heading: 'Siguiente', detail, whatsappLink: 'https://wa.me/59171234567', ...handlers});

    await fireEvent.click(screen.getByRole('button', {name: 'Atendido'}));

    expect(handlers.onAttended).toHaveBeenCalledOnce();
    expect(screen.getByRole('link', {name: 'Recordarle por WhatsApp'})).toHaveAttribute('href', 'https://wa.me/59171234567');
  });

  it('un turno atendido solo se puede deshacer', () => {
    render(BookingDetail, {heading: 'Turno', detail: {...detail, status: 'Attended'}, whatsappLink: '', ...actions()});

    expect(screen.getByRole('button', {name: 'Deshacer'})).toBeInTheDocument();
    expect(screen.queryByRole('button', {name: 'Cancelar turno'})).toBeNull();
  });
});

describe('BlockDetail', () => {
  it('muestra las horas y permite quitar el bloqueo', async () => {
    const onRemove = vi.fn();
    render(BlockDetail, {barberName: 'Marco', start, end: '2026-11-17T11:30:00-04:00', reason: '', onRemove, onClose: vi.fn()});

    await fireEvent.click(screen.getByRole('button', {name: 'Quitar bloqueo'}));

    expect(screen.getByText('10:30 – 11:30')).toBeInTheDocument();
    expect(screen.getByText(/Sin motivo/)).toBeInTheDocument();
    expect(onRemove).toHaveBeenCalledOnce();
  });
});

describe('Ticket', () => {
  it('muestra el número con ceros, la hora y si está cancelado', () => {
    render(Ticket, {ticket: {number: 7, serviceName: 'Barba', barberName: 'Rodri', start, priceCents: 3000}, cancelled: true});

    expect(screen.getByText('0007')).toBeInTheDocument();
    expect(screen.getByText('10:30')).toBeInTheDocument();
    expect(screen.getByText('Cancelado')).toBeInTheDocument();
  });
});
