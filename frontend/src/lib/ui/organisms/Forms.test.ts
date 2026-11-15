import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import BarberForm from './BarberForm.svelte';
import BlockForm from './BlockForm.svelte';
import ServiceForm from './ServiceForm.svelte';
import StaffBookingForm from './StaffBookingForm.svelte';

const services = [{id: 's1', name: 'Degradado'}, {id: 's2', name: 'Barba'}];
const barbers = [{id: 'b1', name: 'Lucho'}];

describe('BarberForm', () => {
  it('envía nombre, servicios marcados y la cuenta si se pidió', async () => {
    const onSubmit = vi.fn();
    render(BarberForm, {services, withAccount: true, onSubmit, onCancel: vi.fn()});

    await fireEvent.input(screen.getByLabelText('Nombre'), {target: {value: 'Marco'}});
    await fireEvent.click(screen.getByLabelText('Degradado'));
    await fireEvent.click(screen.getByLabelText(/Darle acceso al panel/));
    await fireEvent.input(screen.getByLabelText('Correo'), {target: {value: 'marco@barberia.bo'}});
    await fireEvent.input(screen.getByLabelText('Contraseña'), {target: {value: 'barbero-seguro'}});
    await fireEvent.click(screen.getByRole('button', {name: 'Guardar'}));

    expect(onSubmit).toHaveBeenCalledWith({
      name: 'Marco',
      isActive: true,
      serviceIds: ['s1'],
      account: {email: 'marco@barberia.bo', password: 'barbero-seguro'},
    });
  });
});

describe('ServiceForm', () => {
  it('pide un precio válido antes de guardar', async () => {
    const onSubmit = vi.fn();
    render(ServiceForm, {onSubmit, onCancel: vi.fn()});

    await fireEvent.input(screen.getByLabelText('Nombre'), {target: {value: 'Cejas'}});
    await fireEvent.input(screen.getByLabelText('Precio (Bs)'), {target: {value: 'gratis'}});
    await fireEvent.click(screen.getByRole('button', {name: 'Guardar'}));

    expect(onSubmit).not.toHaveBeenCalled();
    expect(screen.getByText(/Escribe el precio en bolivianos/)).toBeInTheDocument();
  });

  it('envía el precio en centavos', async () => {
    const onSubmit = vi.fn();
    render(ServiceForm, {onSubmit, onCancel: vi.fn()});

    await fireEvent.input(screen.getByLabelText('Nombre'), {target: {value: 'Cejas'}});
    await fireEvent.input(screen.getByLabelText('Precio (Bs)'), {target: {value: '15'}});
    await fireEvent.click(screen.getByRole('button', {name: 'Guardar'}));

    expect(onSubmit).toHaveBeenCalledWith({name: 'Cejas', durationMinutes: 30, priceCents: 1500, isActive: true});
  });
});

describe('BlockForm', () => {
  it('propone el almuerzo con el primer barbero', async () => {
    const onSubmit = vi.fn();
    render(BlockForm, {barbers, onSubmit, onCancel: vi.fn()});

    await fireEvent.click(screen.getByRole('button', {name: 'Bloquear'}));

    expect(onSubmit).toHaveBeenCalledWith({barberId: 'b1', from: '13:00', to: '14:00', reason: 'Almuerzo'});
  });
});

describe('StaffBookingForm', () => {
  it('anota un turno sin reserva con lo elegido', async () => {
    const onSubmit = vi.fn();
    render(StaffBookingForm, {services, barbers, defaultTime: '10:30', onSubmit, onCancel: vi.fn()});

    await fireEvent.input(screen.getByLabelText('Nombre'), {target: {value: 'Juan'}});
    await fireEvent.click(screen.getByRole('button', {name: 'Anotar'}));

    expect(onSubmit).toHaveBeenCalledWith(expect.objectContaining({
      serviceId: 's1',
      barberId: 'b1',
      time: '10:30',
      customerName: 'Juan',
    }));
  });
});
