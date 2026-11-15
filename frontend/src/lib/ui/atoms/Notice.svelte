<!-- Aviso dentro de la página: error en rojo o confirmación en azul, leído por lectores de pantalla. -->
<script lang="ts">
  import type {Snippet} from 'svelte';

  type Tone = 'error' | 'success';

  interface Props {
    tone?: Tone;
    children: Snippet;
  }

  const {tone = 'error', children}: Props = $props();
</script>

{#if tone === 'error'}
  <p class="notice error" role="alert">{@render children()}</p>
{:else}
  <p class="notice success" role="status">{@render children()}</p>
{/if}

<style>
  .notice {
    margin: 0;
    padding: var(--space-3) var(--space-4);
    border: var(--line-thin) solid currentColor;
    background: var(--color-sheet);
  }

  .error {
    color: var(--color-action);
  }

  .success {
    color: var(--color-selected);
  }
</style>
