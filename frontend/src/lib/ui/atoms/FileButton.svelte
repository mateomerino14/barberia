<!-- Botón que abre el selector de fotos del teléfono o la computadora; se ve igual que los demás botones. -->
<script lang="ts">
  import type {Snippet} from 'svelte';

  interface Props {
    accept?: string;
    disabled?: boolean;
    onPick: (file: File) => void;
    children: Snippet;
  }

  const {accept = 'image/jpeg,image/png,image/webp', disabled = false, onPick, children}: Props = $props();

  /** Avisa qué archivo eligió y limpia el campo para poder elegir el mismo otra vez. */
  function change(event: Event): void {
    const input = event.currentTarget as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      onPick(file);
    }
    input.value = '';
  }
</script>

<label class="file" class:disabled>
  <input class="hidden" type="file" {accept} {disabled} onchange={change} />
  {@render children()}
</label>

<style>
  .file {
    min-height: var(--touch-min);
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0 var(--space-4);
    border: var(--line-firm) solid var(--color-ink);
    background: transparent;
    color: var(--color-ink);
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

  .file:hover {
    background: var(--color-sheet);
  }

  .file:focus-within {
    outline: var(--line-bold) solid var(--color-selected);
    outline-offset: var(--line-bold);
  }

  .disabled {
    border-style: dashed;
    border-color: var(--color-rule);
    color: var(--color-disabled);
    cursor: not-allowed;
  }

  .hidden {
    position: absolute;
    width: 1px;
    height: 1px;
    overflow: hidden;
    clip-path: inset(50%);
    white-space: nowrap;
  }
</style>
