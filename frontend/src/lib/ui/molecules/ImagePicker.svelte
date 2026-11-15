<!-- Una imagen con su vista previa y los botones para subirla o quitarla: logo o foto de un barbero. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import FileButton from '#lib/ui/atoms/FileButton.svelte';

  type Shape = 'square' | 'portrait';

  interface Props {
    src: string | null;
    alt: string;
    emptyText: string;
    pickLabel: string;
    shape?: Shape;
    busy?: boolean;
    onPick: (file: File) => void;
    onRemove: () => void;
  }

  const {src, alt, emptyText, pickLabel, shape = 'square', busy = false, onPick, onRemove}: Props = $props();
</script>

<div class="picker">
  <div class="frame {shape}">
    {#if src}
      <img class="image" {src} {alt} />
    {:else}
      <span class="empty">{emptyText}</span>
    {/if}
  </div>
  <div class="actions">
    <FileButton disabled={busy} {onPick}>{pickLabel}</FileButton>
    {#if src}
      <Button variant="danger" disabled={busy} onclick={onRemove}>Quitar</Button>
    {/if}
  </div>
</div>

<style>
  .picker {
    display: flex;
    flex-wrap: wrap;
    align-items: flex-end;
    gap: var(--space-4);
  }

  .frame {
    display: grid;
    place-items: center;
    border: var(--line-firm) solid var(--color-ink);
    background: var(--color-paper);
    overflow: hidden;
  }

  .square {
    width: 8rem;
    aspect-ratio: 1;
  }

  .portrait {
    width: 8rem;
    aspect-ratio: 4 / 5;
  }

  .image {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .empty {
    padding: var(--space-2);
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
    text-align: center;
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
  }
</style>
