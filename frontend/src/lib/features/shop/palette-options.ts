import type {Palette} from '#lib/core/theme.svelte.ts';

/** Nombre y descripción de cada paleta para mostrarla al dueño. */
export const paletteOptions: {id: Palette; name: string; description: string}[] = [
  {id: 'classic', name: 'Poste clásico', description: 'Rojo y azul del poste de barbería.'},
  {id: 'bottleGreen', name: 'Verde botella', description: 'Sobrio, de barbería inglesa.'},
  {id: 'wine', name: 'Vino', description: 'Elegante y cálido.'},
  {id: 'mustard', name: 'Mostaza', description: 'Retro, de los años sesenta.'},
  {id: 'graphite', name: 'Grafito', description: 'Moderno y urbano, con naranja.'},
];
