import {render, screen} from '@testing-library/svelte';
import {createRawSnippet} from 'svelte';
import {describe, expect, it} from 'vitest';
import Button from './Button.svelte';

const label = createRawSnippet(() => ({render: () => '<span>Reservar turno</span>'}));

describe('Button', () => {
  it('muestra el texto y es un botón normal por defecto', () => {
    render(Button, {children: label});

    const button = screen.getByRole('button', {name: 'Reservar turno'});
    expect(button).toHaveAttribute('type', 'button');
    expect(button).toHaveClass('primary');
  });

  it('usa la variante secundaria cuando se pide', () => {
    render(Button, {children: label, variant: 'secondary'});

    expect(screen.getByRole('button')).toHaveClass('secondary');
  });
});
