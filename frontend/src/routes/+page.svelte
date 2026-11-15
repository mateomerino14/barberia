<!-- Presentación de la barbería para el cliente: portada con el poste en 3D, precios, equipo, horario y ubicación. -->
<script lang="ts">
  import {resolve} from '$app/paths';
  import {onMount} from 'svelte';
  import {theme} from '#lib/core/theme.svelte.ts';
  import {LandingState} from '#lib/features/landing/state.svelte.ts';
  import {imageUrl} from '#lib/features/media/api.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';
  import StepTitle from '#lib/ui/molecules/StepTitle.svelte';
  import LandingHero from '#lib/ui/organisms/LandingHero.svelte';
  import LocationCard from '#lib/ui/organisms/LocationCard.svelte';
  import PriceList from '#lib/ui/organisms/PriceList.svelte';
  import ShopGallery from '#lib/ui/organisms/ShopGallery.svelte';
  import TeamGrid from '#lib/ui/organisms/TeamGrid.svelte';
  import WeekSchedule from '#lib/ui/organisms/WeekSchedule.svelte';

  const landing = new LandingState();
  const reserveHref = resolve('/reservar');

  onMount(() => {
    landing.load();
  });

  type Section = 'prices' | 'team' | 'place' | 'hours' | 'map';

  /** Secciones que se muestran, en orden; el equipo y el local solo si hay qué mostrar. */
  function sections(): Section[] {
    const shown: Section[] = ['prices'];
    if (landing.barbers.length > 0) {
      shown.push('team');
    }
    if (photos().length > 0) {
      shown.push('place');
    }
    shown.push('hours', 'map');
    return shown;
  }

  /** Número de la sección como "01", según las que se muestran. */
  function step(section: Section): string {
    return String(sections().indexOf(section) + 1).padStart(2, '0');
  }

  /** Fotos del local. */
  function photos(): string[] {
    return (landing.shop?.galleryImageIds ?? []).map(imageUrl);
  }

  /** Equipo con la dirección de la foto de cada uno. */
  function team() {
    return landing.barbers.map((barber) => {
      let photoSrc: string | null = null;
      if (barber.photoImageId) {
        photoSrc = imageUrl(barber.photoImageId);
      }
      return {id: barber.id, name: barber.name, photoSrc};
    });
  }

  /** Logo de la barbería, o null. */
  function logo(): string | null {
    const id = landing.shop?.logoImageId;
    if (!id) {
      return null;
    }
    return imageUrl(id);
  }
</script>

<svelte:head>
  <title>{landing.shop?.name ?? 'Barbería'}</title>
</svelte:head>

<LandingHero
  name={landing.shop?.name ?? 'Barbería'}
  logoSrc={logo()}
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
      <StepTitle step={step('prices')} title="Servicios y precios" />
      <PriceList services={landing.services} />
    </section>

    {#if landing.barbers.length > 0}
      <section class="section">
        <StepTitle step={step('team')} title="El equipo" />
        <TeamGrid members={team()} />
      </section>
    {/if}

    {#if photos().length > 0}
      <section class="section">
        <StepTitle step={step('place')} title="El local" />
        <ShopGallery sources={photos()} shopName={landing.shop.name} />
      </section>
    {/if}

    <section class="section">
      <StepTitle step={step('hours')} title="Horario" />
      <WeekSchedule rows={landing.schedule} />
    </section>

    <section class="section">
      <StepTitle step={step('map')} title="Dónde estamos" />
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
