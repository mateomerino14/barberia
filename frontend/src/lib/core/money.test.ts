import {describe, expect, it} from 'vitest';
import {formatPrice, parsePrice, priceInput} from './money.ts';

describe('money', () => {
  it('muestra bolivianos con centavos solo cuando hay', () => {
    expect(formatPrice(5000)).toBe('Bs 50');
    expect(formatPrice(5250)).toBe('Bs 52,50');
    expect(formatPrice(5205)).toBe('Bs 52,05');
  });

  it('lee precios escritos con coma o punto', () => {
    expect(parsePrice('52,5')).toBe(5250);
    expect(parsePrice(' 40 ')).toBe(4000);
    expect(parsePrice('52.50')).toBe(5250);
  });

  it('rechaza lo que no es un número', () => {
    expect(parsePrice('')).toBeNull();
    expect(parsePrice('cincuenta')).toBeNull();
  });

  it('prepara el precio para editarlo', () => {
    expect(priceInput(5250)).toBe('52,50');
  });
});
