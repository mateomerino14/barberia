import {fireEvent, render, screen} from '@testing-library/svelte';
import {createRawSnippet} from 'svelte';
import {describe, expect, it, vi} from 'vitest';
import FormField from './FormField.svelte';
import HoursRow from './HoursRow.svelte';
import ImagePicker from './ImagePicker.svelte';
import PageTitle from './PageTitle.svelte';
import StepTitle from './StepTitle.svelte';
import ThemeToggle from './ThemeToggle.svelte';

describe('FormField', () => {
  it('une la etiqueta con su campo y muestra el error', () => {
    const input = createRawSnippet(() => ({render: () => '<input id="campo-nombre" />'}));
    render(FormField, {label: 'Nombre', id: 'campo-nombre', error: 'Falta el nombre', children: input});

    expect(screen.getByLabelText('Nombre')).toBeInTheDocument();
    expect(screen.getByText('Falta el nombre')).toBeInTheDocument();
  });
});

describe('HoursRow', () => {
  it('muestra el día cerrado sin horas', () => {
    render(HoursRow, {dayName: 'Lunes', open: false});

    expect(screen.getByText('Lunes')).toBeInTheDocument();
    expect(screen.getByText('Cerrado')).toBeInTheDocument();
  });

  it('muestra las horas cuando abre', () => {
    render(HoursRow, {dayName: 'Martes', open: true, opens: '09:00', closes: '19:00'});

    expect(screen.getByDisplayValue('09:00')).toBeInTheDocument();
    expect(screen.getByDisplayValue('19:00')).toBeInTheDocument();
  });
});

describe('PageTitle y StepTitle', () => {
  it('muestran el título con su número o su antetítulo', () => {
    render(PageTitle, {kicker: 'El equipo', title: 'Barberos'});
    render(StepTitle, {step: '02', title: 'El equipo'});

    expect(screen.getByRole('heading', {name: 'Barberos'})).toBeInTheDocument();
    expect(screen.getByRole('heading', {name: /02\s*El equipo/})).toBeInTheDocument();
  });
});

describe('ThemeToggle', () => {
  it('dice a qué modo cambia y avisa al tocarlo', async () => {
    const onToggle = vi.fn();
    render(ThemeToggle, {dark: true, onToggle});

    await fireEvent.click(screen.getByRole('button', {name: 'Cambiar a modo claro'}));

    expect(onToggle).toHaveBeenCalledOnce();
  });
});

describe('ImagePicker', () => {
  it('sin imagen muestra el texto vacío y no ofrece quitar', () => {
    render(ImagePicker, {
      src: null,
      alt: 'Logo',
      emptyText: 'Sin logo',
      pickLabel: 'Subir logo',
      onPick: vi.fn(),
      onRemove: vi.fn(),
    });

    expect(screen.getByText('Sin logo')).toBeInTheDocument();
    expect(screen.queryByRole('button', {name: 'Quitar'})).toBeNull();
  });

  it('con imagen la muestra y permite quitarla', async () => {
    const onRemove = vi.fn();
    render(ImagePicker, {
      src: 'https://api.test/api/images/1',
      alt: 'Logo de Don Lucho',
      emptyText: 'Sin logo',
      pickLabel: 'Subir logo',
      onPick: vi.fn(),
      onRemove,
    });

    await fireEvent.click(screen.getByRole('button', {name: 'Quitar'}));

    expect(screen.getByRole('img', {name: 'Logo de Don Lucho'})).toBeInTheDocument();
    expect(onRemove).toHaveBeenCalledOnce();
  });
});
