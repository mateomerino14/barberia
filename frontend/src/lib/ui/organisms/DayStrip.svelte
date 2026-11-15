<!-- Tira de días para reservar: tachados los cerrados y los llenos, y cuántas horas libres le quedan a cada uno. -->
<script lang="ts">
  import {todayInBolivia, weekdayOf} from '#lib/core/time.ts';

  interface DayOption {
    date: string;
    open: boolean;
    freeStarts: number | null;
  }

  interface Props {
    days: DayOption[];
    selected: string;
    showCounts?: boolean;
    onSelect: (date: string) => void;
  }

  const {days, selected, showCounts = false, onSelect}: Props = $props();
  const shortNames = {monday: 'Lun', tuesday: 'Mar', wednesday: 'Mié', thursday: 'Jue', friday: 'Vie', saturday: 'Sáb', sunday: 'Dom'};
  const dayStart = 8;
  // Espacio fijo para que todos los días midan lo mismo aunque todavía no se sepan sus horas libres.
  const unknown = '\u00a0';

  /** Nombre corto del día, o "Hoy". */
  function label(date: string): string {
    if (date === todayInBolivia()) {
      return 'Hoy';
    }
    return shortNames[weekdayOf(date)];
  }

  /** Indica si el día abre pero ya no le queda ninguna hora libre. */
  function isFull(day: DayOption): boolean {
    return day.open && day.freeStarts === 0;
  }

  /** Estado del día en palabras: cerrado, lleno, horas libres o nada si todavía no se sabe. */
  function status(day: DayOption): string {
    if (!day.open) {
      return 'Cerrado';
    }
    if (isFull(day)) {
      return 'Lleno';
    }
    if (showCounts && day.freeStarts !== null) {
      if (day.freeStarts === 1) {
        return '1 libre';
      }
      return `${day.freeStarts} libres`;
    }
    return unknown;
  }
</script>

<div class="strip" role="group" aria-label="Días">
  {#each days as day (day.date)}
    <button
      class="day"
      class:full={isFull(day)}
      type="button"
      disabled={!day.open || isFull(day)}
      aria-pressed={day.date === selected}
      aria-label="{label(day.date)} {day.date} {status(day)}"
      onclick={() => onSelect(day.date)}
    >
      <span class="name">{label(day.date)}</span>
      <strong class="number">{Number(day.date.slice(dayStart))}</strong>
      <span class="status">{status(day)}</span>
    </button>
  {/each}
</div>

<style>
  .strip {
    display: flex;
    gap: var(--space-2);
    overflow-x: auto;
    padding-bottom: var(--space-2);
  }

  .day {
    flex: 0 0 4.75rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-1);
    padding: var(--space-2) 0;
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    color: var(--color-ink);
    font-family: var(--font-sans);
    cursor: pointer;
  }

  .day[aria-pressed='true'] {
    border-color: var(--color-selected);
    background: var(--color-selected);
    color: var(--color-sheet);
  }

  .day:disabled {
    border-style: dashed;
    border-color: var(--color-rule);
    background: transparent;
    color: var(--color-disabled);
    cursor: not-allowed;
  }

  .day.full {
    border-color: var(--color-action);
    color: var(--color-action);
  }

  .day:disabled .number {
    text-decoration: line-through;
  }

  .name {
    font-size: var(--text-xs);
    text-transform: uppercase;
  }

  .number {
    font-family: var(--font-mono);
    font-size: var(--text-xl);
  }

  .status {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    white-space: nowrap;
  }
</style>
