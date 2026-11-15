<!-- Página de error: dirección que no existe o algo que falló. -->
<script lang="ts">
  import {page} from '$app/state';
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';

  const notFound = 404;
</script>

<svelte:head>
  <title>Barbería · Página no encontrada</title>
</svelte:head>

<PoleStripe />
<main class="error">
  <span class="code">{page.status}</span>
  {#if page.status === notFound}
    <h1 class="title">Esta página no existe</h1>
    <p>Puede que el enlace esté incompleto.</p>
  {:else}
    <h1 class="title">Algo salió mal</h1>
    <p>Intenta de nuevo en un momento.</p>
  {/if}
  <a class="link" href="/">Ir a reservar un turno</a>
</main>

<style>
  .error {
    max-width: 30rem;
    margin: 0 auto;
    padding: var(--space-6) var(--space-5);
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .code {
    font-family: var(--font-mono);
    color: var(--color-action);
  }

  .title {
    margin: 0;
    font-size: var(--text-display);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 0.95;
    text-transform: uppercase;
  }

  p {
    margin: 0;
    color: var(--color-ink-soft);
  }

  .link {
    color: var(--color-action);
    font-weight: 700;
  }
</style>
