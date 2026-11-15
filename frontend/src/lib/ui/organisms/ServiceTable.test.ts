import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import ServiceTable from './ServiceTable.svelte';

const services = [
  {id: 'a', name: 'Degradado', durationMinutes: 45, priceCents: 5000, isActive: true},
  {id: 'b', name: 'Cejas', durationMinutes: 15, priceCents: 1500, isActive: false},
];

describe('ServiceTable', () => {
  it('muestra precio, duración y marca los ocultos', () => {
    render(ServiceTable, {services, onEdit: vi.fn()});

    expect(screen.getByText('Bs 50')).toBeInTheDocument();
    expect(screen.getByText('45 min')).toBeInTheDocument();
    expect(screen.getAllByText('Oculto')).toHaveLength(1);
  });

  it('avisa qué servicio se quiere editar', async () => {
    const onEdit = vi.fn();
    render(ServiceTable, {services, onEdit});

    await fireEvent.click(screen.getByRole('button', {name: 'Editar Cejas'}));

    expect(onEdit).toHaveBeenCalledWith('b');
  });

  it('invita a crear el primero cuando no hay', () => {
    render(ServiceTable, {services: [], onEdit: vi.fn()});

    expect(screen.getByText(/Todavía no hay servicios/)).toBeInTheDocument();
  });
});
