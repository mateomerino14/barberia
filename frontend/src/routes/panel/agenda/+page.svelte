<!-- Agenda del día; por ahora avisa que llega con el módulo de reservas. -->
<script lang="ts">
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';
</script>

<svelte:head>
  <title>Agenda · Barbería</title>
</svelte:head>

<PageTitle kicker="Hoy" title="Agenda" />
<p class="soon">La agenda del día aparece aquí cuando empiecen a llegar reservas.</p>

<style>
  .soon {
    margin: 0;
    color: var(--color-ink-soft);
  }
</style>
