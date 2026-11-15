<!-- Tira de días para elegir la fecha; los días que no abre la barbería aparecen tachados. -->
<script lang="ts">
  import {todayInBolivia, weekdayOf} from '#lib/core/time.ts';

  interface DayOption {
    date: string;
    open: boolean;
  }

  interface Props {
    days: DayOption[];
    selected: string;
    onSelect: (date: string) => void;
  }

  const {days, selected, onSelect}: Props = $props();
  const shortNames = {monday: 'Lun', tuesday: 'Mar', wednesday: 'Mié', thursday: 'Jue', friday: 'Vie', saturday: 'Sáb', sunday: 'Dom'};
  const dayStart = 8;

  /** Nombre corto del día, o "Hoy". */
  function label(date: string): string {
    if (date === todayInBolivia()) {
      return 'Hoy';
    }
    return shortNames[weekdayOf(date)];
  }
</script>

<div class="strip" role="group" aria-label="Días">
  {#each days as day (day.date)}
    <button
      class="day"
      type="button"
      disabled={!day.open}
      aria-pressed={day.date === selected}
      aria-label="{label(day.date)} {day.date}"
      onclick={() => onSelect(day.date)}
    >
      <span class="name">{label(day.date)}</span>
      <strong class="number">{Number(day.date.slice(dayStart))}</strong>
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
    flex: 0 0 4rem;
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
</style>
