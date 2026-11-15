<!-- Etiqueta corta en letra de máquina para estados como "Oculto" o "Atendido". -->
<script lang="ts">
  import type {Snippet} from 'svelte';

  type Tone = 'neutral' | 'action' | 'selected';

  interface Props {
    tone?: Tone;
    children: Snippet;
  }

  const {tone = 'neutral', children}: Props = $props();
</script>

<span class="badge {tone}">{@render children()}</span>

<style>
  .badge {
    display: inline-block;
    padding: var(--space-1) var(--space-2);
    border: var(--line-thin) solid currentColor;
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    letter-spacing: 0.06em;
    text-transform: uppercase;
  }

  .neutral {
    color: var(--color-ink-soft);
  }

  .action {
    color: var(--color-action);
  }

  .selected {
    color: var(--color-selected);
  }
</style>
