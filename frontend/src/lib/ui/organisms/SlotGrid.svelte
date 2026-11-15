<!-- Horas libres agrupadas en mañana y tarde, para elegir una. -->
<script lang="ts">
  import {clockTime} from '#lib/core/time.ts';

  interface SlotOption {
    start: string;
  }

  interface Props {
    slots: SlotOption[];
    selected: string | null;
    loading?: boolean;
    emptyText: string;
    onSelect: (start: string) => void;
  }

  const {slots, selected, loading = false, emptyText, onSelect}: Props = $props();
  const noon = '12:00';

  /** Horas de la mañana. */
  function morning(): SlotOption[] {
    return slots.filter((slot) => clockTime(slot.start) < noon);
  }

  /** Horas de la tarde. */
  function afternoon(): SlotOption[] {
    return slots.filter((slot) => clockTime(slot.start) >= noon);
  }
</script>

{#if loading}
  <p class="empty" aria-live="polite">Buscando horas libres…</p>
{:else if slots.length === 0}
  <p class="empty" aria-live="polite">{emptyText}</p>
{:else}
  {#each [{label: 'Mañana', items: morning()}, {label: 'Tarde', items: afternoon()}] as group (group.label)}
    {#if group.items.length > 0}
      <div class="group">
        <span class="label">{group.label}</span>
        <div class="grid">
          {#each group.items as slot (slot.start)}
            <button
              class="slot"
              type="button"
              aria-pressed={slot.start === selected}
              onclick={() => onSelect(slot.start)}
            >
              {clockTime(slot.start)}
            </button>
          {/each}
        </div>
      </div>
    {/if}
  {/each}
{/if}

<style>
  .group {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
    margin-bottom: var(--space-3);
  }

  .label {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: var(--color-ink-soft);
  }

  .grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(4.5rem, 1fr));
    gap: var(--space-2);
  }

  .slot {
    min-height: var(--touch-min);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    color: var(--color-ink);
    font-family: var(--font-mono);
    font-size: var(--text-md);
    cursor: pointer;
  }

  .slot[aria-pressed='true'] {
    border-color: var(--color-selected);
    background: var(--color-selected);
    color: var(--color-sheet);
    font-weight: 600;
  }

  .empty {
    margin: 0;
    color: var(--color-ink-soft);
  }
</style>
