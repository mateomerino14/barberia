<!-- Estructura de las páginas del panel: barra superior y contenido centrado. -->
<script lang="ts">
  import type {Snippet} from 'svelte';

  interface Props {
    header: Snippet;
    children: Snippet;
  }

  const {header, children}: Props = $props();
</script>

<div class="panel">
  {@render header()}
  <main class="content">
    {@render children()}
  </main>
</div>

<style>
  .panel {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
  }

  .content {
    width: 100%;
    max-width: var(--content-max);
    margin: 0 auto;
    padding: var(--space-5);
    display: flex;
    flex-direction: column;
    gap: var(--space-5);
  }
</style>
