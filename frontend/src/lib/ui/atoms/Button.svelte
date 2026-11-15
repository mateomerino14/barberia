<!-- Botón de texto: rojo para la acción principal, tinta para confirmar, contorno para lo secundario. -->
<script lang="ts">
  import type {Snippet} from 'svelte';
  import type {HTMLButtonAttributes} from 'svelte/elements';

  type Variant = 'primary' | 'dark' | 'secondary' | 'danger';

  interface Props extends HTMLButtonAttributes {
    variant?: Variant;
    wide?: boolean;
    children: Snippet;
  }

  const {variant = 'primary', wide = false, type = 'button', children, ...rest}: Props = $props();
</script>

<button class="button {variant}" class:wide {type} {...rest}>
  {@render children()}
</button>

<style>
  .button {
    min-height: var(--touch-min);
    padding: 0 var(--space-5);
    border: var(--line-firm) solid var(--color-ink);
    border-radius: 0;
    font-family: var(--font-sans);
    font-size: var(--text-lg);
    font-weight: 800;
    font-stretch: var(--stretch-button);
    letter-spacing: 0.04em;
    line-height: 1;
    text-transform: uppercase;
    white-space: nowrap;
    cursor: pointer;
  }

  .wide {
    width: 100%;
  }

  .primary {
    border-color: var(--color-action);
    background: var(--color-action);
    color: var(--color-on-accent);
  }

  .primary:hover {
    border-color: var(--color-action-dark);
    background: var(--color-action-dark);
  }

  .dark {
    background: var(--color-ink);
    color: var(--color-sheet);
  }

  .secondary {
    background: transparent;
    color: var(--color-ink);
  }

  .secondary:hover {
    background: var(--color-sheet);
  }

  .danger {
    border-color: var(--color-action);
    background: transparent;
    color: var(--color-action);
  }

  .danger:hover {
    background: var(--color-sheet);
  }

  .button:disabled {
    border-style: dashed;
    border-color: var(--color-rule);
    background: transparent;
    color: var(--color-disabled);
    cursor: not-allowed;
  }
</style>
