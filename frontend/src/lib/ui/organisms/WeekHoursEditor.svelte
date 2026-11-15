<!-- Editor del horario semanal: un renglón por día, de lunes a domingo. -->
<script lang="ts">
  import {weekdayName, weekdays, type Weekday} from '#lib/core/time.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import HoursRow from '#lib/ui/molecules/HoursRow.svelte';

  interface DayHours {
    day: Weekday;
    opens: string;
    closes: string;
  }

  interface Props {
    hours: DayHours[];
    saving?: boolean;
    saveLabel?: string;
    onSave: (hours: DayHours[]) => void;
  }

  const {hours, saving = false, saveLabel = 'Guardar horario', onSave}: Props = $props();

  const defaultOpens = '09:00';
  const defaultCloses = '19:00';

  /** Arma un renglón por día a partir del horario guardado. */
  function toRows(saved: DayHours[]) {
    return weekdays.map((day) => {
      const found = saved.find((item) => item.day === day);
      if (found) {
        const length = defaultOpens.length;
        return {day, open: true, opens: found.opens.slice(0, length), closes: found.closes.slice(0, length)};
      }
      return {day, open: false, opens: defaultOpens, closes: defaultCloses};
    });
  }

  const rows = $derived(toRows(hours));

  /** Envía solo los días marcados como abiertos. */
  function save(event: SubmitEvent): void {
    event.preventDefault();
    onSave(rows.filter((row) => row.open).map((row) => ({day: row.day, opens: row.opens, closes: row.closes})));
  }
</script>

<form class="week" onsubmit={save}>
  {#each rows as row (row.day)}
    <HoursRow dayName={weekdayName(row.day)} bind:open={row.open} bind:opens={row.opens} bind:closes={row.closes} />
  {/each}
  <div class="actions">
    <Button type="submit" disabled={saving}>{saveLabel}</Button>
  </div>
</form>

<style>
  .week {
    display: flex;
    flex-direction: column;
  }

  .actions {
    padding-top: var(--space-4);
  }
</style>
