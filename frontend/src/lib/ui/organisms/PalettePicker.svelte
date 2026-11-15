<!-- Tarjetas para elegir la paleta de la barbería; cada una se pinta con sus propios colores. -->
<script lang="ts">
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';

  interface PaletteOption {
    id: string;
    name: string;
    description: string;
  }

  interface Props {
    options: PaletteOption[];
    selected: string;
    shopName: string;
    onSelect: (id: string) => void;
  }

  const {options, selected, shopName, onSelect}: Props = $props();
</script>

<fieldset class="grid">
  <legend class="legend">Paleta de colores</legend>
  {#each options as option (option.id)}
    <label class="card" class:chosen={option.id === selected} data-palette={option.id}>
      <span class="bar">{shopName}</span>
      <PoleStripe />
      <span class="body">
        <span class="chips" aria-hidden="true">
          <span class="chip">10:00</span>
          <span class="chip on">10:30</span>
        </span>
        <span class="cta" aria-hidden="true">Reservar turno</span>
        <span class="pick">
          <input
            type="radio"
            name="palette"
            value={option.id}
            checked={option.id === selected}
            onchange={() => onSelect(option.id)}
          />
          <strong>{option.name}</strong>
        </span>
        <span class="description">{option.description}</span>
      </span>
    </label>
  {/each}
</fieldset>

<style>
  .grid {
    margin: 0;
    padding: 0;
    border: 0;
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(10rem, 1fr));
    gap: var(--space-3);
  }

  .legend {
    margin-bottom: var(--space-2);
    font-size: var(--text-sm);
    font-weight: 700;
  }

  .card {
    display: flex;
    flex-direction: column;
    border: var(--line-thin) solid var(--color-rule);
    background: var(--color-paper);
    color: var(--color-ink);
    cursor: pointer;
  }

  .chosen {
    outline: 3px solid var(--color-ink);
    outline-offset: 2px;
  }

  .bar {
    padding: var(--space-3);
    background: var(--color-bar);
    color: var(--color-on-bar);
    font-size: var(--text-lg);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 1;
    text-transform: uppercase;
  }

  .body {
    padding: var(--space-3);
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .chips {
    display: flex;
    gap: var(--space-1);
  }

  .chip {
    flex: 1;
    padding: var(--space-1) 0;
    border: var(--line-thin) solid var(--color-ink);
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    text-align: center;
  }

  .chip.on {
    border-color: var(--color-selected);
    background: var(--color-selected);
    color: var(--color-on-accent);
  }

  .cta {
    padding: var(--space-2) 0;
    background: var(--color-action);
    color: var(--color-on-accent);
    font-size: var(--text-sm);
    font-weight: 800;
    font-stretch: var(--stretch-button);
    text-align: center;
    text-transform: uppercase;
  }

  .pick {
    min-height: var(--touch-min);
    display: flex;
    align-items: center;
    gap: var(--space-2);
  }

  input {
    width: 1.25rem;
    height: 1.25rem;
    accent-color: var(--color-selected);
  }

  .description {
    font-size: var(--text-xs);
    color: var(--color-ink-soft);
  }
</style>
