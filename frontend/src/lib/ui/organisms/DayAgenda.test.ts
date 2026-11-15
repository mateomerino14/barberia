import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import DayAgenda from './DayAgenda.svelte';

const columns = [
  {id: 'a', name: 'Lucho', opens: '2026-11-17T09:00:00-04:00', closes: '2026-11-17T19:00:00-04:00'},
  {id: 'b', name: 'Marco', opens: null, closes: null},
];
const items = [
  {
    id: 'x',
    barberId: 'a',
    start: '2026-11-17T10:00:00-04:00',
    end: '2026-11-17T10:45:00-04:00',
    title: 'Degradado',
    subtitle: 'Juan',
    tone: 'next' as const,
  },
];
const blocks = [
  {
    id: 'k',
    barberId: 'a',
    start: '2026-11-17T12:00:00-04:00',
    end: '2026-11-17T13:00:00-04:00',
    reason: 'Almuerzo',
  },
];

describe('DayAgenda', () => {
  it('muestra las columnas, el horario y quién no trabaja', () => {
    render(DayAgenda, {columns, items, blocks, nowTime: null, selectedId: null, onSelect: vi.fn(), onRemoveBlock: vi.fn()});

    expect(screen.getByText('09:00 – 19:00')).toBeInTheDocument();
    expect(screen.getByText('No trabaja')).toBeInTheDocument();
    expect(screen.getByText('Siguiente')).toBeInTheDocument();
  });

  it('ubica el turno en su fila y avisa al elegirlo', async () => {
    const onSelect = vi.fn();
    render(DayAgenda, {columns, items, blocks, nowTime: null, selectedId: null, onSelect, onRemoveBlock: vi.fn()});

    const booking = screen.getByRole('button', {name: /Degradado/});
    expect(booking.style.gridRow).toBe('5 / span 3');
    expect(booking.style.gridColumn).toBe('2');
    await fireEvent.click(booking);
    expect(onSelect).toHaveBeenCalledWith('x');
  });

  it('dibuja la hora actual solo si cae en el día mostrado', () => {
    const props = {columns, items, blocks, selectedId: null, onSelect: vi.fn(), onRemoveBlock: vi.fn()};
    const {unmount} = render(DayAgenda, {...props, nowTime: '10:40'});
    expect(screen.getByRole('img', {name: 'Hora actual 10:40'})).toBeInTheDocument();
    unmount();

    render(DayAgenda, {...props, nowTime: '22:00'});
    expect(screen.queryByRole('img')).not.toBeInTheDocument();
  });

  it('permite quitar un bloqueo', async () => {
    const onRemoveBlock = vi.fn();
    render(DayAgenda, {columns, items, blocks, nowTime: null, selectedId: null, onSelect: vi.fn(), onRemoveBlock});

    await fireEvent.click(screen.getByRole('button', {name: /Quitar bloqueo/}));
    expect(onRemoveBlock).toHaveBeenCalledWith('k');
  });
});
