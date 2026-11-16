import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import ForgotPasswordForm from './ForgotPasswordForm.svelte';

describe('ForgotPasswordForm', () => {
  it('pide el enlace para el correo escrito', async () => {
    const onSubmit = vi.fn();
    render(ForgotPasswordForm, {minutes: 30, onSubmit, onBack: vi.fn()});

    await fireEvent.input(screen.getByLabelText('Correo'), {target: {value: 'dueno@barberia.bo'}});
    await fireEvent.click(screen.getByRole('button', {name: 'Mandarme el enlace'}));

    expect(onSubmit).toHaveBeenCalledWith('dueno@barberia.bo');
  });

  it('enviado, dice lo mismo exista o no la cuenta y cuánto dura el enlace', () => {
    render(ForgotPasswordForm, {sent: true, minutes: 30, onSubmit: vi.fn(), onBack: vi.fn()});

    expect(screen.getByText(/Si ese correo tiene cuenta/)).toHaveTextContent('Vence en 30 minutos');
    expect(screen.queryByLabelText('Correo')).toBeNull();
  });

  it('permite volver a ingresar', async () => {
    const onBack = vi.fn();
    render(ForgotPasswordForm, {minutes: 30, onSubmit: vi.fn(), onBack});

    await fireEvent.click(screen.getByRole('button', {name: 'Volver a ingresar'}));

    expect(onBack).toHaveBeenCalledOnce();
  });
});
