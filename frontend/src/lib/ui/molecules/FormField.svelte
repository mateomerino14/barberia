<!-- Campo de formulario: etiqueta, control, ayuda y error, enlazados para lectores de pantalla. -->
<script lang="ts">
  import type {Snippet} from 'svelte';

  interface Props {
    label: string;
    id: string;
    hint?: string;
    error?: string | null;
    children: Snippet;
  }

  const {label, id, hint = '', error = null, children}: Props = $props();
</script>

<div class="field">
  <label class="label" for={id}>{label}</label>
  {@render children()}
  {#if hint}
    <span class="hint" id="{id}-hint">{hint}</span>
  {/if}
  {#if error}
    <span class="error" id="{id}-error">{error}</span>
  {/if}
</div>

<style>
  .field {
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .label {
    font-size: var(--text-sm);
    font-weight: 700;
  }

  .hint {
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  .error {
    font-size: var(--text-sm);
    font-weight: 600;
    color: var(--color-action);
  }
</style>
