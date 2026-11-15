<!-- Campo de texto de la identidad: borde de tinta, fondo de hoja y foco azul. -->
<script lang="ts">
  import type {HTMLInputAttributes} from 'svelte/elements';

  interface Props extends Omit<HTMLInputAttributes, 'value'> {
    value?: string | number | null;
    invalid?: boolean;
    mono?: boolean;
  }

  let {value = $bindable(''), invalid = false, mono = false, ...rest}: Props = $props();
</script>

<input class="input" class:mono class:invalid aria-invalid={invalid} bind:value {...rest} />

<style>
  .input {
    min-height: var(--touch-min);
    width: 100%;
    padding: 0 var(--space-3);
    border: var(--line-thin) solid var(--color-ink);
    border-radius: 0;
    background: var(--color-sheet);
    color: var(--color-ink);
    font-family: var(--font-sans);
    font-size: var(--text-md);
  }

  .mono {
    font-family: var(--font-mono);
  }

  .invalid {
    border-color: var(--color-action);
    border-width: var(--line-bold);
  }

  .input:disabled {
    border-style: dashed;
    border-color: var(--color-rule);
    color: var(--color-disabled);
  }
</style>
