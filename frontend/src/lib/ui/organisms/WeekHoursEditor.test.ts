import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import WeekHoursEditor from './WeekHoursEditor.svelte';

describe('WeekHoursEditor', () => {
  it('muestra los siete días y marca abiertos solo los guardados', () => {
    render(WeekHoursEditor, {hours: [{day: 'saturday', opens: '09:00', closes: '20:00'}], onSave: vi.fn()});

    expect(screen.getAllByRole('checkbox')).toHaveLength(7);
    expect(screen.getByRole('checkbox', {name: 'Sábado'})).toBeChecked();
    expect(screen.getByRole('checkbox', {name: 'Lunes'})).not.toBeChecked();
  });

  it('envía solo los días abiertos, también los que se abren al editar', async () => {
    const onSave = vi.fn();
    render(WeekHoursEditor, {hours: [{day: 'saturday', opens: '09:00', closes: '20:00'}], onSave});

    await fireEvent.click(screen.getByRole('checkbox', {name: 'Lunes'}));
    await fireEvent.click(screen.getByRole('button', {name: 'Guardar horario'}));

    expect(onSave).toHaveBeenCalledWith([
      {day: 'monday', opens: '09:00', closes: '19:00'},
      {day: 'saturday', opens: '09:00', closes: '20:00'},
    ]);
  });
});
