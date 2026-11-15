<!-- Presentación de la barbería para el cliente: portada con el poste en 3D, precios, equipo, horario y ubicación. -->
<script lang="ts">
  import {resolve} from '$app/paths';
  import {onMount} from 'svelte';
  import {theme} from '#lib/core/theme.svelte.ts';
  import {LandingState} from '#lib/features/landing/state.svelte.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';
  import StepTitle from '#lib/ui/molecules/StepTitle.svelte';
  import LandingHero from '#lib/ui/organisms/LandingHero.svelte';
  import LocationCard from '#lib/ui/organisms/LocationCard.svelte';
  import PriceList from '#lib/ui/organisms/PriceList.svelte';
  import TeamGrid from '#lib/ui/organisms/TeamGrid.svelte';
  import WeekSchedule from '#lib/ui/organisms/WeekSchedule.svelte';

  const landing = new LandingState();
  const reserveHref = resolve('/reservar');

  onMount(() => {
    landing.load();
  });
</script>

<svelte:head>
  <title>{landing.shop?.name ?? 'Barbería'}</title>
</svelte:head>

<LandingHero
  name={landing.shop?.name ?? 'Barbería'}
  place={landing.shop?.address ?? ''}
  welcome={landing.shop?.welcomeMessage ?? ''}
  status={landing.status}
  {reserveHref}
  dark={theme.isDark}
  onToggleTheme={() => theme.toggle()}
/>
<PoleStripe size="thick" />

<main class="page">
  {#if landing.error}
    <Notice>{landing.error}</Notice>
  {/if}

  {#if landing.shop}
    <section class="section" id="precios">
      <StepTitle step="01" title="Servicios y precios" />
      <PriceList services={landing.services} />
    </section>

    {#if landing.barbers.length > 0}
      <section class="section">
        <StepTitle step="02" title="El equipo" />
        <TeamGrid barbers={landing.barbers} />
      </section>
    {/if}

    <section class="section">
      <StepTitle step="03" title="Horario" />
      <WeekSchedule rows={landing.schedule} />
    </section>

    <section class="section">
      <StepTitle step="04" title="Dónde estamos" />
      <LocationCard name={landing.shop.name} address={landing.shop.address} phone={landing.shop.phone} />
    </section>
  {:else if landing.loading}
    <p class="muted">Cargando…</p>
  {/if}
</main>

<footer class="cta">
  <span class="cta-text">Elige servicio, barbero y hora en un minuto.</span>
  <Button href={reserveHref}>Reservar</Button>
</footer>

<style>
  .page {
    max-width: var(--content-max);
    margin: 0 auto;
    padding: var(--space-6) var(--space-5);
    display: flex;
    flex-direction: column;
    gap: var(--space-6);
  }

  .section {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
    scroll-margin-top: var(--space-5);
  }

  .muted {
    color: var(--color-ink-soft);
  }

  .cta {
    position: sticky;
    bottom: 0;
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-5);
    background: var(--color-sheet);
    border-top: var(--line-bold) solid var(--color-ink);
  }

  .cta-text {
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }
</style>
