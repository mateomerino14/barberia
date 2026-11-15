<!-- Casilla con su texto, de al menos 44 px de alto para tocarla con el dedo. -->
<script lang="ts">
  import type {Snippet} from 'svelte';

  interface Props {
    checked?: boolean;
    disabled?: boolean;
    children: Snippet;
  }

  let {checked = $bindable(), disabled = false, children}: Props = $props();
</script>

<label class="checkbox">
  <input type="checkbox" bind:checked {disabled} />
  <span>{@render children()}</span>
</label>

<style>
  .checkbox {
    min-height: var(--touch-min);
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    cursor: pointer;
  }

  input {
    width: 1.25rem;
    height: 1.25rem;
    accent-color: var(--color-selected);
  }
</style>
