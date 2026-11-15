<!-- Equipo de barberos: lista, alta, edición, horario y ausencias. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import {formatDateRange} from '#lib/core/time.ts';
  import {BarbersState} from '#lib/features/barbers/state.svelte.ts';
  import Badge from '#lib/ui/atoms/Badge.svelte';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';
  import BarberForm from '#lib/ui/organisms/BarberForm.svelte';
  import PanelAccessForm from '#lib/ui/organisms/PanelAccessForm.svelte';
  import DateEntriesEditor from '#lib/ui/organisms/DateEntriesEditor.svelte';
  import WeekHoursEditor from '#lib/ui/organisms/WeekHoursEditor.svelte';

  const team = new BarbersState();

  onMount(() => {
    team.load();
  });

  /** Ausencias del barbero abierto, listas para mostrar. */
  function absences() {
    return (team.selected?.timeOff ?? []).map((item) => ({
      key: item.id,
      label: formatDateRange(item.from, item.to),
      reason: item.reason,
    }));
  }

  /** Nombres de los servicios que hace un barbero. */
  function serviceNames(ids: string[]): string {
    return team.services.filter((service) => ids.includes(service.id)).map((service) => service.name).join(', ');
  }
</script>

<svelte:head>
  <title>Barberos · Barbería</title>
</svelte:head>

<PageTitle kicker="El equipo" title="Barberos">
  {#snippet actions()}
    {#if !team.creating}
      <Button onclick={() => team.startCreate()}>Nuevo barbero</Button>
    {/if}
  {/snippet}
</PageTitle>

{#if team.error}
  <Notice>{team.error}</Notice>
{/if}
{#if team.notice}
  <Notice tone="success">{team.notice}</Notice>
{/if}

{#if team.creating}
  <section class="sheet" aria-label="Barbero nuevo">
    <BarberForm
      services={team.services}
      withAccount
      saving={team.saving}
      onSubmit={(values) => team.save(values)}
      onCancel={() => team.close()}
    />
  </section>
{/if}

<div class="layout">
  <ul class="list" aria-label="Barberos">
    {#each team.barbers as barber (barber.id)}
      <li>
        <button
          class="person"
          class:current={barber.id === team.selectedId}
          type="button"
          onclick={() => team.open(barber.id)}
        >
          <span class="initials" aria-hidden="true">{barber.name.slice(0, 2).toUpperCase()}</span>
          <span class="who">
            <strong>{barber.name}</strong>
            <span class="services">{serviceNames(barber.serviceIds) || 'Sin servicios'}</span>
          </span>
          {#if !barber.isActive}
            <Badge>Inactivo</Badge>
          {/if}
        </button>
      </li>
    {:else}
      {#if !team.loading}
        <li class="empty">Todavía no hay barberos.</li>
      {/if}
    {/each}
  </ul>

  {#if team.selected}
    <div class="detail">
      <section class="sheet" aria-label="Datos de {team.selected.name}">
        <h2 class="subtitle">{team.selected.name}</h2>
        <BarberForm
          services={team.services}
          initial={team.selected}
          saving={team.saving}
          onSubmit={(values) => team.save(values)}
          onCancel={() => team.close()}
        />
      </section>
      <section class="block">
        <h2 class="subtitle">Horario</h2>
        <WeekHoursEditor
          hours={team.selectedSchedule}
          saving={team.saving}
          onSave={(hours) => team.saveSchedule(hours)}
        />
      </section>
      <section class="block">
        <h2 class="subtitle">Acceso al panel</h2>
        {#key team.selectedId}
          <PanelAccessForm
            email={team.selectedEmail}
            saving={team.saving}
            onSubmit={(email, password) => team.saveAccess(email, password)}
          />
        {/key}
      </section>
      <section class="block">
        <h2 class="subtitle">Ausencias</h2>
        <DateEntriesEditor
          idPrefix="absence"
          range
          entries={absences()}
          emptyText="Sin ausencias registradas."
          reasonPlaceholder="Vacaciones"
          busy={team.saving}
          onAdd={(from, to, reason) => team.addAbsence(from, to, reason)}
          onRemove={(id) => team.removeAbsence(id)}
        />
      </section>
    </div>
  {/if}
</div>

<style>
  .layout {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-5);
    align-items: flex-start;
  }

  .list {
    flex: 1 1 16rem;
    margin: 0;
    padding: 0;
    list-style: none;
    border-top: var(--line-thin) solid var(--color-ink);
  }

  .person {
    width: 100%;
    min-height: var(--touch-min);
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-2);
    border: 0;
    border-bottom: var(--line-thin) solid var(--color-rule);
    background: transparent;
    color: var(--color-ink);
    font-family: var(--font-sans);
    font-size: var(--text-md);
    text-align: left;
    cursor: pointer;
  }

  .person.current {
    background: var(--color-sheet);
    box-shadow: inset 4px 0 0 var(--color-selected);
  }

  .initials {
    width: var(--touch-min);
    height: var(--touch-min);
    display: grid;
    place-items: center;
    background: var(--color-ink);
    color: var(--color-sheet);
    font-weight: 800;
  }

  .who {
    flex: 1;
    display: flex;
    flex-direction: column;
  }

  .services {
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  .empty {
    padding: var(--space-3) 0;
    color: var(--color-ink-soft);
  }

  .detail {
    flex: 999 1 30rem;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-5);
  }

  .sheet {
    padding: var(--space-5);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

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
</style>
