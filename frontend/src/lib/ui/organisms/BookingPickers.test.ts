import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import BarberPicker from './BarberPicker.svelte';
import DayStrip from './DayStrip.svelte';
import SlotGrid from './SlotGrid.svelte';

describe('SlotGrid', () => {
  it('separa mañana y tarde y marca la hora elegida', async () => {
    const onSelect = vi.fn();
    const slots = [{start: '2026-11-15T10:45:00-04:00'}, {start: '2026-11-15T14:00:00-04:00'}];
    render(SlotGrid, {slots, selected: slots[0].start, emptyText: 'Nada', onSelect});

    expect(screen.getByText('Mañana')).toBeInTheDocument();
    expect(screen.getByText('Tarde')).toBeInTheDocument();
    expect(screen.getByRole('button', {name: '10:45'})).toHaveAttribute('aria-pressed', 'true');
    await fireEvent.click(screen.getByRole('button', {name: '14:00'}));
    expect(onSelect).toHaveBeenCalledWith(slots[1].start);
  });

  it('avisa cuando no hay horas', () => {
    render(SlotGrid, {slots: [], selected: null, emptyText: 'No quedan horas libres', onSelect: vi.fn()});

    expect(screen.getByText('No quedan horas libres')).toBeInTheDocument();
  });
});

describe('DayStrip', () => {
  it('no deja elegir los días cerrados', () => {
    render(DayStrip, {
      days: [{date: '2026-11-15', open: true, freeStarts: null}, {date: '2026-11-16', open: false, freeStarts: null}],
      selected: '2026-11-15',
      onSelect: vi.fn(),
    });

    const buttons = screen.getAllByRole('button');
    expect(buttons[0]).toBeEnabled();
    expect(buttons[1]).toBeDisabled();
    expect(screen.getByText('Cerrado')).toBeInTheDocument();
  });

  it('marca los días llenos aunque todavía no se haya elegido servicio', () => {
    render(DayStrip, {
      days: [{date: '2026-11-17', open: true, freeStarts: 9}, {date: '2026-11-18', open: true, freeStarts: 0}],
      selected: '2026-11-17',
      onSelect: vi.fn(),
    });

    expect(screen.getByRole('button', {name: /2026-11-18 Lleno/})).toBeDisabled();
    expect(screen.queryByText('9 libres')).toBeNull();
  });

  it('con servicio elegido muestra cuántas horas libres quedan', () => {
    render(DayStrip, {
      days: [{date: '2026-11-17', open: true, freeStarts: 9}, {date: '2026-11-19', open: true, freeStarts: 1}],
      selected: '2026-11-17',
      showCounts: true,
      onSelect: vi.fn(),
    });

    expect(screen.getByText('9 libres')).toBeInTheDocument();
    expect(screen.getByText('1 libre')).toBeInTheDocument();
  });
});

describe('BarberPicker', () => {
  it('ofrece "Cualquiera" y avisa null al elegirlo', async () => {
    const onSelect = vi.fn();
    render(BarberPicker, {barbers: [{id: 'a', name: 'Lucho'}], selectedId: 'a', onSelect});

    expect(screen.getByRole('button', {name: /Lucho/})).toHaveAttribute('aria-pressed', 'true');
    await fireEvent.click(screen.getByRole('button', {name: /Cualquiera/}));
    expect(onSelect).toHaveBeenCalledWith(null);
  });
});
