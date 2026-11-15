import {render, screen} from '@testing-library/svelte';
import {describe, expect, it, vi} from 'vitest';
import BarberPole3D from './BarberPole3D.svelte';
import LandingHero from './LandingHero.svelte';
import LocationCard from './LocationCard.svelte';
import ShopGallery from './ShopGallery.svelte';

describe('LandingHero', () => {
  it('muestra el nombre, si está abierta, el logo y lleva a reservar', () => {
    render(LandingHero, {
      name: 'Don Lucho',
      logoSrc: 'https://api.test/api/images/logo',
      place: 'Sopocachi, La Paz',
      welcome: 'Tu turno sin filas.',
      status: {open: true, text: 'Abierto · cierra 20:00'},
      reserveHref: '/reservar',
      dark: false,
      onToggleTheme: vi.fn(),
    });

    expect(screen.getByRole('heading', {level: 1, name: 'Don Lucho'})).toBeInTheDocument();
    expect(screen.getByText('Abierto · cierra 20:00')).toBeInTheDocument();
    expect(screen.getByRole('img', {name: 'Logo de Don Lucho'})).toBeInTheDocument();
    expect(screen.getByRole('link', {name: 'Reservar turno'})).toHaveAttribute('href', '/reservar');
  });
});

describe('BarberPole3D', () => {
  it('sin 3D en el navegador muestra el poste plano', async () => {
    render(BarberPole3D);

    expect(screen.getByRole('img', {name: 'Poste de barbería girando'})).toBeInTheDocument();
    await vi.waitFor(() => expect(document.querySelector('.flat')).not.toBeNull());
  });
});

describe('LocationCard', () => {
  it('ofrece cómo llegar con el mapa y escribir por WhatsApp', () => {
    render(LocationCard, {name: 'Don Lucho', address: 'Av. 20 de Octubre 2045', phone: '71234567'});

    expect(screen.getByRole('link', {name: 'Cómo llegar'})).toHaveAttribute(
      'href',
      'https://www.google.com/maps/search/?api=1&query=Av.%2020%20de%20Octubre%202045',
    );
    expect(screen.getByRole('link', {name: 'WhatsApp'}).getAttribute('href')).toContain('https://wa.me/59171234567');
  });

  it('sin teléfono no muestra WhatsApp', () => {
    render(LocationCard, {name: 'Don Lucho', address: 'Av. 20 de Octubre', phone: ''});

    expect(screen.queryByRole('link', {name: 'WhatsApp'})).toBeNull();
  });
});

describe('ShopGallery', () => {
  it('muestra cada foto con su descripción', () => {
    render(ShopGallery, {sources: ['a.webp', 'b.webp'], shopName: 'Don Lucho'});

    expect(screen.getAllByRole('img')).toHaveLength(2);
    expect(screen.getByRole('img', {name: 'Foto 1 de Don Lucho'})).toBeInTheDocument();
  });
});
