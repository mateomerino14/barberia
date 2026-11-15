import {fireEvent, render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import {paletteOptions} from '#lib/features/shop/palette-options.ts';
import PalettePicker from './PalettePicker.svelte';

describe('PalettePicker', () => {
  it('marca la paleta elegida y avisa al cambiarla', async () => {
    const onSelect = vi.fn();
    render(PalettePicker, {options: paletteOptions, selected: 'classic', shopName: 'Barbería', onSelect});

    expect(screen.getByRole('radio', {name: /Poste clásico/})).toBeChecked();
    await fireEvent.click(screen.getByRole('radio', {name: /Vino/}));

    expect(onSelect).toHaveBeenCalledWith('wine');
  });
});
