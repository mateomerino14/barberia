import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import PasswordForm from './PasswordForm.svelte';

/** Llena los tres campos del formulario. */
async function fill(current: string, next: string, repeat: string): Promise<void> {
  await fireEvent.input(screen.getByLabelText('Contraseña actual'), {target: {value: current}});
  await fireEvent.input(screen.getByLabelText('Contraseña nueva'), {target: {value: next}});
  await fireEvent.input(screen.getByLabelText('Repite la contraseña nueva'), {target: {value: repeat}});
}

describe('PasswordForm', () => {
  it('no envía si la nueva no coincide al repetirla', async () => {
    const onSubmit = vi.fn(async () => true);
    render(PasswordForm, {onSubmit});

    await fill('actual-123', 'nueva-1234', 'otra-1234');
    await fireEvent.click(screen.getByRole('button', {name: 'Cambiar contraseña'}));

    expect(onSubmit).not.toHaveBeenCalled();
    expect(screen.getByText('No coincide con la contraseña nueva.')).toBeInTheDocument();
  });

  it('envía la actual y la nueva cuando coinciden', async () => {
    const onSubmit = vi.fn(async () => true);
    render(PasswordForm, {onSubmit});

    await fill('actual-123', 'nueva-1234', 'nueva-1234');
    await fireEvent.click(screen.getByRole('button', {name: 'Cambiar contraseña'}));

    expect(onSubmit).toHaveBeenCalledWith('actual-123', 'nueva-1234');
  });
});

describe('PanelAccessForm', () => {
  it('pide correo solo cuando el barbero todavía no tiene acceso', async () => {
    const {default: PanelAccessForm} = await import('./PanelAccessForm.svelte');
    const onSubmit = vi.fn(async () => true);
    const {unmount} = render(PanelAccessForm, {email: null, onSubmit});
    expect(screen.getByLabelText('Correo')).toBeInTheDocument();
    unmount();

    render(PanelAccessForm, {email: 'marco@barberia.bo', onSubmit});
    expect(screen.queryByLabelText('Correo')).toBeNull();
    expect(screen.getByText('marco@barberia.bo')).toBeInTheDocument();
  });
});
