<!-- Fotos del local: las que ya están, con botón para quitar cada una, y un espacio para agregar otra. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import FileButton from '#lib/ui/atoms/FileButton.svelte';

  interface Photo {
    id: string;
    src: string;
  }

  interface Props {
    photos: Photo[];
    max: number;
    busy?: boolean;
    onAdd: (file: File) => void;
    onRemove: (id: string) => void;
  }

  const {photos, max, busy = false, onAdd, onRemove}: Props = $props();
</script>

<div class="gallery">
  <ul class="grid" aria-label="Fotos del local">
    {#each photos as photo, index (photo.id)}
      <li class="item">
        <img class="image" src={photo.src} alt="Foto {index + 1} del local" />
        <Button variant="danger" disabled={busy} onclick={() => onRemove(photo.id)}>Quitar</Button>
      </li>
    {/each}
    {#if photos.length < max}
      <li class="item add">
        <span class="slot">{photos.length} de {max}</span>
        <FileButton disabled={busy} onPick={onAdd}>Agregar foto</FileButton>
      </li>
    {/if}
  </ul>
</div>

<style>
  .grid {
    margin: 0;
    padding: 0;
    list-style: none;
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(10rem, 1fr));
    gap: var(--space-4);
  }

  .item {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .image {
    width: 100%;
    aspect-ratio: 4 / 3;
    object-fit: cover;
    border: var(--line-firm) solid var(--color-ink);
  }

  .slot {
    aspect-ratio: 4 / 3;
    display: grid;
    place-items: center;
    border: var(--line-firm) dashed var(--color-rule);
    font-family: var(--font-mono);
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }
</style>
