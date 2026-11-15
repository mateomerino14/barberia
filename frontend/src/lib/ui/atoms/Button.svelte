<!-- Botón de texto de la identidad: rojo para la acción principal, contorno para la secundaria. -->
<script lang="ts">
  import type {Snippet} from 'svelte';
  import type {HTMLButtonAttributes} from 'svelte/elements';

  type Variant = 'primary' | 'secondary';

  interface Props extends HTMLButtonAttributes {
    variant?: Variant;
    children: Snippet;
  }

  const {variant = 'primary', type = 'button', children, ...rest}: Props = $props();
</script>

<button class="button {variant}" {type} {...rest}>
  {@render children()}
</button>

<style>
  .button {
    min-height: var(--touch-min);
    padding: 0 var(--space-5);
    border: var(--line-firm) solid var(--color-ink);
    font-family: var(--font-sans);
    font-size: var(--text-lg);
    font-weight: 800;
    font-stretch: var(--stretch-button);
    letter-spacing: 0.04em;
    text-transform: uppercase;
    white-space: nowrap;
    cursor: pointer;
  }

  .primary {
    border-color: var(--color-action);
    background: var(--color-action);
    color: var(--color-sheet);
  }

  .primary:hover {
    border-color: var(--color-action-dark);
    background: var(--color-action-dark);
  }

  .secondary {
    background: transparent;
    color: var(--color-ink);
  }

  .button:disabled {
    border-style: dashed;
    border-color: var(--color-rule);
    background: transparent;
    color: var(--color-disabled);
    cursor: not-allowed;
  }
</style>
