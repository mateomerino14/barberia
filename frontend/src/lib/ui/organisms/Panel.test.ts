import {fireEvent, render, screen} from '@testing-library/svelte';
import {createRawSnippet} from 'svelte';
import {describe, expect, it, vi} from 'vitest';
import PanelLayout from '../templates/PanelLayout.svelte';
import GalleryEditor from './GalleryEditor.svelte';
import PanelHeader from './PanelHeader.svelte';
import ServicePicker from './ServicePicker.svelte';

describe('PanelHeader', () => {
  it('marca la página actual y permite salir', async () => {
    const onLogout = vi.fn();
    const links = [{href: '/panel/agenda', label: 'Agenda'}, {href: '/panel/cuenta', label: 'Mi cuenta'}];
    render(PanelHeader, {
      shopName: 'Don Lucho',
      links,
      currentPath: '/panel/cuenta',
      userLabel: 'dueno@barberia.bo',
      dark: false,
      onToggleTheme: vi.fn(),
      onLogout,
    });

    await fireEvent.click(screen.getByRole('button', {name: 'Salir'}));

    expect(screen.getByRole('link', {name: 'Mi cuenta'})).toHaveAttribute('aria-current', 'page');
    expect(screen.getByRole('link', {name: 'Agenda'})).not.toHaveAttribute('aria-current');
    expect(onLogout).toHaveBeenCalledOnce();
  });
});

describe('PanelLayout', () => {
  it('pone la barra arriba y la página en el contenido principal', () => {
    const header = createRawSnippet(() => ({render: () => '<header>Barra</header>'}));
    const children = createRawSnippet(() => ({render: () => '<p>Agenda del día</p>'}));
    render(PanelLayout, {header, children});

    expect(screen.getByRole('main')).toHaveTextContent('Agenda del día');
    expect(screen.getByRole('banner')).toHaveTextContent('Barra');
  });
});

describe('GalleryEditor', () => {
  it('cuenta las fotos y deja quitar cada una', async () => {
    const onRemove = vi.fn();
    const photos = [{id: 'f1', src: 'https://api.test/api/images/f1'}];
    render(GalleryEditor, {photos, max: 6, onAdd: vi.fn(), onRemove});

    await fireEvent.click(screen.getByRole('button', {name: 'Quitar'}));

    expect(screen.getByText('1 de 6')).toBeInTheDocument();
    expect(onRemove).toHaveBeenCalledWith('f1');
  });

  it('con el máximo de fotos ya no ofrece agregar', () => {
    const photos = [{id: 'f1', src: 'a'}, {id: 'f2', src: 'b'}];
    render(GalleryEditor, {photos, max: 2, onAdd: vi.fn(), onRemove: vi.fn()});

    expect(screen.queryByText('Agregar foto')).toBeNull();
  });
});

describe('ServicePicker', () => {
  it('muestra precio y duración y avisa el servicio elegido', async () => {
    const onSelect = vi.fn();
    render(ServicePicker, {services: [{id: 's1', name: 'Degradado', durationMinutes: 45, priceCents: 5000}], selectedId: null, onSelect});

    await fireEvent.click(screen.getByRole('radio', {name: /Degradado/}));

    expect(screen.getByText('Bs 50')).toBeInTheDocument();
    expect(onSelect).toHaveBeenCalledWith('s1');
  });

  it('avisa si todavía no hay servicios', () => {
    render(ServicePicker, {services: [], selectedId: null, onSelect: vi.fn()});

    expect(screen.getByText(/todavía no publicó sus servicios/)).toBeInTheDocument();
  });
});
