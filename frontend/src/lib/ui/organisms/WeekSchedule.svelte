<!-- Horario de la semana con el día de hoy resaltado y los días cerrados tachados. -->
<script lang="ts">
  import {fromApiTime, weekdayName, type Weekday} from '#lib/core/time.ts';

  interface ScheduleRow {
    day: Weekday;
    hours: {opens: string; closes: string} | null;
    today: boolean;
  }

  interface Props {
    rows: ScheduleRow[];
  }

  const {rows}: Props = $props();
</script>

<ul class="week">
  {#each rows as row (row.day)}
    <li class="day" class:today={row.today} class:closed={!row.hours}>
      <span class="name">
        {weekdayName(row.day)}
        {#if row.today}<span class="tag">Hoy</span>{/if}
      </span>
      {#if row.hours}
        <span class="hours">{fromApiTime(row.hours.opens)} – {fromApiTime(row.hours.closes)}</span>
      {:else}
        <span class="hours">Cerrado</span>
      {/if}
    </li>
  {/each}
</ul>

<style>
  .week {
    margin: 0;
    padding: 0;
    list-style: none;
    border-top: var(--line-bold) solid var(--color-ink);
  }

  .day {
    display: flex;
    justify-content: space-between;
    align-items: center;
    min-height: var(--touch-min);
    padding: 0 var(--space-3);
    border-bottom: var(--line-thin) solid var(--color-rule);
  }

  .name {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    font-weight: 700;
  }

  .hours {
    font-family: var(--font-mono);
  }

  .tag {
    padding: 0 var(--space-2);
    background: var(--color-action);
    color: var(--color-on-accent);
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    text-transform: uppercase;
  }

  .today {
    background: var(--color-sheet);
    border-left: var(--space-1) solid var(--color-action);
  }

  .closed .name,
  .closed .hours {
    color: var(--color-disabled);
    text-decoration: line-through;
  }
</style>
