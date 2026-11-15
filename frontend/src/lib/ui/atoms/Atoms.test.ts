import {fireEvent, render, screen} from '@testing-library/svelte';
import {createRawSnippet} from 'svelte';
import {describe, expect, it, vi} from 'vitest';
import Badge from './Badge.svelte';
import Checkbox from './Checkbox.svelte';
import FileButton from './FileButton.svelte';
import Notice from './Notice.svelte';
import PoleStripe from './PoleStripe.svelte';

/** Arma un contenido de texto para pasarlo como hijo. */
function text(value: string) {
  return createRawSnippet(() => ({render: () => `<span>${value}</span>`}));
}

describe('Notice', () => {
  it('es una alerta cuando avisa un error y un estado cuando confirma', () => {
    const {unmount} = render(Notice, {children: text('No pudimos guardar')});
    expect(screen.getByRole('alert')).toHaveTextContent('No pudimos guardar');
    unmount();

    render(Notice, {tone: 'success', children: text('Guardado')});
    expect(screen.getByRole('status')).toHaveTextContent('Guardado');
  });
});

describe('Badge', () => {
  it('muestra la etiqueta con su tono', () => {
    render(Badge, {tone: 'selected', children: text('Atendido')});

    expect(screen.getByText('Atendido').parentElement).toHaveClass('selected');
  });
});

describe('Checkbox', () => {
  it('se marca al tocar su texto', async () => {
    render(Checkbox, {children: text('Recibe turnos')});

    await fireEvent.click(screen.getByText('Recibe turnos'));

    expect(screen.getByRole('checkbox')).toBeChecked();
  });
});

describe('PoleStripe', () => {
  it('es solo decorativa', () => {
    const {container} = render(PoleStripe, {size: 'thick'});

    expect(container.querySelector('.pole')).toHaveAttribute('aria-hidden', 'true');
  });
});

describe('FileButton', () => {
  it('avisa la foto elegida', async () => {
    const onPick = vi.fn();
    render(FileButton, {onPick, children: text('Subir foto')});
    const file = new File(['x'], 'foto.png', {type: 'image/png'});

    await fireEvent.change(screen.getByLabelText('Subir foto'), {target: {files: [file]}});

    expect(onPick).toHaveBeenCalledWith(file);
  });

  it('solo acepta fotos', () => {
    render(FileButton, {onPick: vi.fn(), children: text('Subir foto')});

    expect(screen.getByLabelText('Subir foto')).toHaveAttribute('accept', 'image/jpeg,image/png,image/webp');
  });
});
