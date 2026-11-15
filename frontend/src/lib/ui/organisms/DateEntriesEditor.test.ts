import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import DateEntriesEditor from './DateEntriesEditor.svelte';

describe('DateEntriesEditor', () => {
  it('agrega un solo día usando la misma fecha como fin', async () => {
    const onAdd = vi.fn();
    render(DateEntriesEditor, {idPrefix: 'x', entries: [], emptyText: 'Nada', onAdd, onRemove: vi.fn()});

    await fireEvent.input(screen.getByLabelText('Fecha'), {target: {value: '2026-12-25'}});
    await fireEvent.input(screen.getByLabelText('Motivo'), {target: {value: 'Navidad'}});
    await fireEvent.click(screen.getByRole('button', {name: 'Agregar'}));

    expect(onAdd).toHaveBeenCalledWith('2026-12-25', '2026-12-25', 'Navidad');
  });

  it('quita la fecha elegida', async () => {
    const onRemove = vi.fn();
    render(DateEntriesEditor, {
      idPrefix: 'x',
      entries: [{key: 'k1', label: '25 dic', reason: 'Navidad'}],
      emptyText: 'Nada',
      onAdd: vi.fn(),
      onRemove,
    });

    await fireEvent.click(screen.getByRole('button', {name: 'Quitar'}));

    expect(onRemove).toHaveBeenCalledWith('k1');
  });
});
