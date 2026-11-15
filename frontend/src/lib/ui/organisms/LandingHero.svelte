<!-- Portada de la barbería: nombre grande, si está abierta, el poste en 3D y el botón para reservar. -->
<script lang="ts">
  import type {OpenStatus} from '#lib/features/landing/opening.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import ThemeToggle from '#lib/ui/molecules/ThemeToggle.svelte';
  import BarberPole3D from './BarberPole3D.svelte';

  interface Props {
    name: string;
    place: string;
    welcome: string;
    status: OpenStatus | null;
    reserveHref: string;
    dark: boolean;
    onToggleTheme: () => void;
  }

  const {name, place, welcome, status, reserveHref, dark, onToggleTheme}: Props = $props();
</script>

<section class="hero">
  <div class="top">
    <span class="kicker">{place}</span>
    <ThemeToggle {dark} onToggle={onToggleTheme} />
  </div>
  <div class="body">
    <div class="copy">
      {#if status}
        <span class="status" class:open={status.open}><span class="dot"></span>{status.text}</span>
      {/if}
      <h1 class="name">{name}</h1>
      {#if welcome}
        <p class="welcome">{welcome}</p>
      {/if}
      <div class="actions">
        <Button href={reserveHref}>Reservar turno</Button>
        <Button variant="onBar" href="#precios">Ver precios</Button>
      </div>
    </div>
    <div class="pole">
      <BarberPole3D />
    </div>
  </div>
</section>

<style>
  .hero {
    background: var(--color-bar);
    color: var(--color-on-bar);
    padding: var(--space-4) 0 var(--space-6);
  }

  .top {
    max-width: var(--content-max);
    margin: 0 auto;
    padding: 0 var(--space-5);
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .kicker {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--color-on-bar-soft);
  }

  .body {
    max-width: var(--content-max);
    margin: 0 auto;
    display: grid;
    grid-template-columns: 1fr;
    gap: var(--space-4);
    padding: var(--space-5) var(--space-5) 0;
  }

  .copy {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
    align-self: center;
  }

  .status {
    align-self: flex-start;
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-1) var(--space-3);
    border: var(--line-thin) solid var(--color-on-bar-soft);
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--color-on-bar-soft);
  }

  .dot {
    width: var(--space-2);
    height: var(--space-2);
    border-radius: 50%;
    background: var(--color-on-bar-soft);
  }

  .open {
    color: var(--color-on-bar);
  }

  .open .dot {
    background: var(--color-open);
  }

  .name {
    margin: 0;
    font-size: clamp(3.5rem, 15vw, 8.5rem);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 0.85;
    text-transform: uppercase;
    overflow-wrap: anywhere;
  }

  .welcome {
    margin: 0;
    max-width: 30rem;
    font-size: var(--text-lg);
    color: var(--color-on-bar-soft);
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  .pole {
    height: 22rem;
  }

  @media (min-width: 48rem) {
    .body {
      grid-template-columns: 3fr 2fr;
      min-height: 32rem;
    }

    .pole {
      height: 32rem;
    }
  }
</style>
