<!-- Lista desplegable con el mismo borde, alto y fondo que los campos de texto. -->
<script lang="ts">
  import type {HTMLSelectAttributes} from 'svelte/elements';

  interface Option {
    value: string;
    label: string;
  }

  interface Props extends Omit<HTMLSelectAttributes, 'value'> {
    options: Option[];
    value?: string;
  }

  let {options, value = $bindable(''), ...rest}: Props = $props();
</script>

<select class="select" bind:value {...rest}>
  {#each options as option (option.value)}
    <option value={option.value}>{option.label}</option>
  {/each}
</select>

<style>
  .select {
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
</style>
