import {describe, expect, it} from 'vitest';
import {fitWithin} from './image.ts';

describe('fitWithin', () => {
  it('achica respetando la proporción', () => {
    expect(fitWithin(4000, 3000, 1600)).toEqual({width: 1600, height: 1200});
    expect(fitWithin(1000, 2000, 1600)).toEqual({width: 800, height: 1600});
  });

  it('no agranda una foto chica', () => {
    expect(fitWithin(600, 400, 1600)).toEqual({width: 600, height: 400});
  });
});
