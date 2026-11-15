import {beforeEach, describe, expect, it} from 'vitest';
import {forgetCustomer, recallCustomer, rememberCustomer} from './customer-memory.ts';

describe('customer memory', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('no devuelve nada si no se guardó', () => {
    expect(recallCustomer()).toBeNull();
  });

  it('recuerda al cliente guardado', () => {
    rememberCustomer({name: 'Diego', phone: '70000000', email: ''});
    expect(recallCustomer()).toEqual({name: 'Diego', phone: '70000000', email: ''});
  });

  it('olvida al cliente guardado', () => {
    rememberCustomer({name: 'Diego', phone: '70000000', email: ''});
    forgetCustomer();
    expect(recallCustomer()).toBeNull();
  });

  it('ignora datos rotos', () => {
    localStorage.setItem('barberia.customer', '{roto');
    expect(recallCustomer()).toBeNull();
  });
});
