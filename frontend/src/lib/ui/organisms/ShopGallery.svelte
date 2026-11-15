<!-- Fotos del local en mosaico: la primera grande y las demás alrededor. -->
<script lang="ts">
  interface Props {
    sources: string[];
    shopName: string;
  }

  const {sources, shopName}: Props = $props();
</script>

<ul class="mosaic">
  {#each sources as src, index (src)}
    <li class="tile" class:lead={index === 0}>
      <img class="image" {src} alt="Foto {index + 1} de {shopName}" loading="lazy" />
    </li>
  {/each}
</ul>

<style>
  .mosaic {
    margin: 0;
    padding: 0;
    list-style: none;
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: var(--space-3);
  }

  .tile {
    border: var(--line-firm) solid var(--color-ink);
    overflow: hidden;
    aspect-ratio: 4 / 3;
  }

  .lead {
    grid-column: span 2;
  }

  .image {
    width: 100%;
    height: 100%;
    display: block;
    object-fit: cover;
  }

  @media (min-width: 48rem) {
    .mosaic {
      grid-template-columns: repeat(4, 1fr);
    }

    .lead {
      grid-row: span 2;
      aspect-ratio: auto;
    }
  }
</style>
