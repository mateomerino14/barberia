<!-- Botones para elegir barbero, con la opción de que atienda cualquiera. -->
<script lang="ts">
  interface BarberOption {
    id: string;
    name: string;
  }

  interface Props {
    barbers: BarberOption[];
    selectedId: string | null;
    onSelect: (id: string | null) => void;
  }

  const {barbers, selectedId, onSelect}: Props = $props();
  const initialsLength = 2;

  /** Iniciales que se muestran en el cuadro del barbero. */
  function initials(name: string): string {
    return name.slice(0, initialsLength).toUpperCase();
  }
</script>

<div class="grid">
  <button class="barber" type="button" aria-pressed={selectedId === null} onclick={() => onSelect(null)}>
    <span class="mark any" aria-hidden="true">?</span>
    Cualquiera
  </button>
  {#each barbers as barber (barber.id)}
    <button class="barber" type="button" aria-pressed={selectedId === barber.id} onclick={() => onSelect(barber.id)}>
      <span class="mark" aria-hidden="true">{initials(barber.name)}</span>
      {barber.name}
    </button>
  {/each}
</div>

<style>
  .grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(5.5rem, 1fr));
    gap: var(--space-2);
  }

  .barber {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-1);
    padding: var(--space-2) var(--space-1);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    color: var(--color-ink);
    font-family: var(--font-sans);
    font-size: var(--text-sm);
    cursor: pointer;
  }

  .barber[aria-pressed='true'] {
    border: var(--line-bold) solid var(--color-selected);
    color: var(--color-selected);
    font-weight: 700;
  }

  .mark {
    width: var(--touch-min);
    height: var(--touch-min);
    display: grid;
    place-items: center;
    background: var(--color-ink);
    color: var(--color-sheet);
    font-size: var(--text-lg);
    font-weight: 800;
  }

  .barber[aria-pressed='true'] .mark {
    background: var(--color-selected);
  }

  .any {
    border: var(--line-thin) dashed var(--color-ink-soft);
    background: transparent;
    color: var(--color-ink);
    font-family: var(--font-mono);
  }
</style>
