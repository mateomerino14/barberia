import {render, screen} from '@testing-library/svelte';
import {describe, expect, it} from 'vitest';
import PriceList from './PriceList.svelte';
import TeamGrid from './TeamGrid.svelte';
import WeekSchedule from './WeekSchedule.svelte';

describe('secciones de la presentación', () => {
  it('muestra cada servicio con su duración y precio', () => {
    render(PriceList, {services: [{id: 'a', name: 'Degradado', durationMinutes: 45, priceCents: 5000}]});

    expect(screen.getByText('Degradado')).toBeInTheDocument();
    expect(screen.getByText('45 min')).toBeInTheDocument();
    expect(screen.getByText('Bs 50')).toBeInTheDocument();
  });

  it('muestra al equipo con sus nombres', () => {
    render(TeamGrid, {barbers: [{id: 'b', name: 'Diego Mamani', serviceIds: [], photoImageId: null}]});

    expect(screen.getByText('Diego Mamani')).toBeInTheDocument();
    expect(screen.getByText('DM')).toBeInTheDocument();
  });

  it('muestra la foto del barbero cuando la tiene', () => {
    render(TeamGrid, {barbers: [{id: 'c', name: 'Marco', serviceIds: [], photoImageId: 'foto-1'}]});

    expect(document.querySelector('img')?.getAttribute('src')).toContain('/api/images/foto-1');
    expect(screen.queryByText('MA')).toBeNull();
  });

  it('marca hoy y los días cerrados', () => {
    const rows = [
      {day: 'monday' as const, hours: {day: 'monday' as const, opens: '09:00:00', closes: '20:00:00'}, today: true},
      {day: 'sunday' as const, hours: null, today: false},
    ];
    render(WeekSchedule, {rows});

    expect(screen.getByText('Hoy')).toBeInTheDocument();
    expect(screen.getByText('09:00 – 20:00')).toBeInTheDocument();
    expect(screen.getByText('Cerrado')).toBeInTheDocument();
  });
});
