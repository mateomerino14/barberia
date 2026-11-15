import {describe, expect, it} from 'vitest';
import {directionsHref} from './maps.ts';
import {initials} from './text.ts';

describe('directionsHref', () => {
  it('arma el enlace de mapas con la dirección', () => {
    expect(directionsHref('Av. 20 de Octubre')).toBe('https://www.google.com/maps/search/?api=1&query=Av.%2020%20de%20Octubre');
  });
});

describe('initials', () => {
  it('toma hasta dos iniciales', () => {
    expect(initials('Diego  Mamani Quispe')).toBe('DM');
  });
});
