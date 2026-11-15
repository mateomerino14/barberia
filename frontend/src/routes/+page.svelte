<!-- Portada provisional mientras se arma la página de reservas. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
</script>

<svelte:head>
  <title>Barbería · Reserva tu turno</title>
</svelte:head>

<div class="pole" aria-hidden="true"></div>
<main class="home">
  <p class="kicker">Sistema de reservas</p>
  <h1 class="title">Barbería</h1>
  <p class="lead">Tu turno, sin filas ni mensajes de ida y vuelta. Muy pronto vas a poder reservar aquí.</p>
  <Button disabled>Reservar turno</Button>
</main>

<style>
  .pole {
    height: var(--space-2);
    background: var(--pole-stripes);
  }

  .home {
    max-width: var(--content-max);
    margin: 0 auto;
    padding: var(--space-6) var(--space-5);
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: var(--space-4);
  }

  .kicker {
    margin: 0;
    font-family: var(--font-mono);
    font-size: var(--text-sm);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--color-ink-soft);
  }

  .title {
    margin: 0;
    font-size: var(--text-display);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 0.9;
    text-transform: uppercase;
  }

  .lead {
    margin: 0;
    max-width: 30rem;
    font-size: var(--text-lg);
  }
</style>
