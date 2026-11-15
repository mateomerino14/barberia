<!-- Horario de atención de la barbería y días cerrados. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import {formatDate} from '#lib/core/time.ts';
  import {ShopState} from '#lib/features/shop/state.svelte.ts';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';
  import DateEntriesEditor from '#lib/ui/organisms/DateEntriesEditor.svelte';
  import WeekHoursEditor from '#lib/ui/organisms/WeekHoursEditor.svelte';

  const shop = new ShopState();

  onMount(() => {
    shop.load();
  });

  /** Fechas cerradas listas para mostrar. */
  function closedEntries() {
    return (shop.shop?.closedDays ?? []).map((day) => ({
      key: day.date,
      label: formatDate(day.date),
      reason: day.reason,
    }));
  }
</script>

<svelte:head>
  <title>Horarios · Barbería</title>
</svelte:head>

<PageTitle kicker="La barbería" title="Horarios" />

{#if shop.error}
  <Notice>{shop.error}</Notice>
{/if}
{#if shop.notice}
  <Notice tone="success">{shop.notice}</Notice>
{/if}

{#if !shop.loading}
  <section class="block">
    <h2 class="subtitle">Horario de atención</h2>
    <p class="help">Los barberos solo pueden trabajar dentro de este horario.</p>
    <WeekHoursEditor hours={shop.openingHours} saving={shop.saving} onSave={(hours) => shop.saveHours(hours)} />
  </section>
  <section class="block">
    <h2 class="subtitle">Días cerrados</h2>
    <p class="help">Feriados o vacaciones de todo el local. Esos días nadie puede reservar.</p>
    <DateEntriesEditor
      idPrefix="closed"
      entries={closedEntries()}
      emptyText="No hay días cerrados por ahora."
      reasonPlaceholder="Feriado"
      busy={shop.saving}
      onAdd={(from, _to, reason) => shop.addClosedDay(from, reason)}
      onRemove={(date) => shop.removeClosedDay(date)}
    />
  </section>
{/if}

<style>
  .block {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .subtitle {
    margin: 0;
    font-size: var(--text-xl);
    font-weight: 800;
    font-stretch: var(--stretch-button);
    text-transform: uppercase;
  }

  .help {
    margin: 0;
    color: var(--color-ink-soft);
  }
</style>
