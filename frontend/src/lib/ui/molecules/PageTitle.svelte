<!-- Título de página con su línea chica en letra de máquina encima. -->
<script lang="ts">
  import type {Snippet} from 'svelte';

  interface Props {
    kicker: string;
    title: string;
    actions?: Snippet;
  }

  const {kicker, title, actions}: Props = $props();
</script>

<header class="page-title">
  <div class="text">
    <span class="kicker">{kicker}</span>
    <h1 class="title">{title}</h1>
  </div>
  {#if actions}
    <div class="actions">{@render actions()}</div>
  {/if}
</header>

<style>
  .page-title {
    display: flex;
    flex-wrap: wrap;
    align-items: flex-end;
    justify-content: space-between;
    gap: var(--space-3);
  }

  .text {
    display: flex;
    flex-direction: column;
  }

  .kicker {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--color-ink-soft);
  }

  .title {
    margin: 0;
    font-size: var(--text-display);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 0.95;
    text-transform: uppercase;
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
  }
</style>
