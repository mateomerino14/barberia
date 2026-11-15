<!-- Agenda del día: columnas por barbero, turno siguiente, turnos sin reserva y bloqueos. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import {session} from '#lib/core/session.svelte.ts';
  import {clockTime, formatLongDate, localIso, nowClockInBolivia, todayInBolivia} from '#lib/core/time.ts';
  import {whatsappHref} from '#lib/core/whatsapp.ts';
  import {AgendaState} from '#lib/features/agenda/state.svelte.ts';
  import type {AgendaBooking} from '#lib/features/agenda/types.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';
  import BlockForm from '#lib/ui/organisms/BlockForm.svelte';
  import BookingDetail from '#lib/ui/organisms/BookingDetail.svelte';
  import DayAgenda from '#lib/ui/organisms/DayAgenda.svelte';
  import StaffBookingForm from '#lib/ui/organisms/StaffBookingForm.svelte';

  const agenda = new AgendaState();
  const refreshMs = 60_000;
  let now = $state(nowClockInBolivia());

  onMount(() => {
    agenda.load();
    const timer = setInterval(() => {
      now = nowClockInBolivia();
    }, refreshMs);
    return () => clearInterval(timer);
  });

  /** Turnos listos para dibujar, marcando el siguiente. */
  function items() {
    const focusedId = agenda.focused?.id;
    return (agenda.agenda?.bookings ?? []).map((booking) => ({
      id: booking.id,
      barberId: booking.barberId,
      start: booking.start,
      end: booking.end,
      title: booking.serviceName,
      subtitle: booking.customerName,
      tone: tone(booking, focusedId),
    }));
  }

  /** Cómo se pinta un turno según su estado. */
  function tone(booking: AgendaBooking, focusedId: string | undefined): 'booked' | 'attended' | 'noShow' | 'next' {
    if (booking.status === 'Attended') {
      return 'attended';
    }
    if (booking.status === 'NoShow') {
      return 'noShow';
    }
    if (booking.id === focusedId) {
      return 'next';
    }
    return 'booked';
  }

  /** Enlace de WhatsApp con el recordatorio del turno, si hay celular. */
  function reminder(booking: AgendaBooking): string {
    if (!booking.customerPhone) {
      return '';
    }
    const time = clockTime(booking.start);
    const message = `Hola ${booking.customerName}, te recordamos tu turno de ${booking.serviceName} a las ${time}.`
      + ' ¡Te esperamos!';
    return whatsappHref(booking.customerPhone, message);
  }

  /** Título del recuadro del turno según si fue elegido o es el siguiente. */
  function detailHeading(): string {
    if (agenda.selectedId) {
      return 'Turno elegido';
    }
    return 'Siguiente';
  }

  /** Hora actual si la agenda muestra hoy. */
  function nowTime(): string | null {
    if (agenda.date === todayInBolivia()) {
      return now;
    }
    return null;
  }
</script>

<svelte:head>
  <title>Agenda · Barbería</title>
</svelte:head>

<PageTitle kicker={formatLongDate(agenda.date)} title="Agenda">
  {#snippet actions()}
    <Button variant="secondary" aria-label="Día anterior" onclick={() => agenda.move(-1)}>‹</Button>
    <Button variant="secondary" onclick={() => agenda.move(0)}>Hoy</Button>
    <Button variant="secondary" aria-label="Día siguiente" onclick={() => agenda.move(1)}>›</Button>
    <Button variant="secondary" onclick={() => agenda.open('block')}>Bloquear horas</Button>
    <Button onclick={() => agenda.open('booking')}>Anotar turno</Button>
  {/snippet}
</PageTitle>

{#if agenda.error}
  <Notice>{agenda.error}</Notice>
{/if}

{#if agenda.form === 'booking' && agenda.agenda}
  <section class="sheet" aria-label="Anotar turno">
    <StaffBookingForm
      services={agenda.services}
      barbers={agenda.agenda.barbers}
      defaultTime={now}
      busy={agenda.busy}
      onSubmit={(values) => agenda.createBooking({...values, start: localIso(agenda.date, values.time)})}
      onCancel={() => agenda.open('none')}
    />
  </section>
{/if}

{#if agenda.form === 'block' && agenda.agenda}
  <section class="sheet" aria-label="Bloquear horas">
    <BlockForm
      barbers={agenda.agenda.barbers}
      busy={agenda.busy}
      onSubmit={(values) => agenda.block({...values, date: agenda.date})}
      onCancel={() => agenda.open('none')}
    />
  </section>
{/if}

<div class="layout">
  <div class="main">
    {#if agenda.loading && !agenda.agenda}
      <p class="muted">Cargando la agenda…</p>
    {:else if agenda.agenda && agenda.agenda.barbers.length > 0}
      <DayAgenda
        columns={agenda.agenda.barbers}
        items={items()}
        blocks={agenda.agenda.blocks}
        nowTime={nowTime()}
        selectedId={agenda.selectedId}
        onSelect={(id) => agenda.select(id)}
        onRemoveBlock={(id) => agenda.unblock(id)}
      />
    {:else}
      <p class="muted">
        {#if session.isOwner}
          Todavía no hay barberos activos. Agrégalos en la página de barberos.
        {:else}
          Tu ficha de barbero no está activa. Habla con el dueño.
        {/if}
      </p>
    {/if}
  </div>
  <aside class="side">
    {#if agenda.focused}
      {@const booking = agenda.focused}
      <BookingDetail
        heading={detailHeading()}
        detail={{...booking, barberName: agenda.barberName(booking.barberId)}}
        whatsappLink={reminder(booking)}
        busy={agenda.busy}
        onAttended={() => agenda.setStatus(booking.id, 'Attended')}
        onNoShow={() => agenda.setStatus(booking.id, 'NoShow')}
        onUndo={() => agenda.setStatus(booking.id, 'Booked')}
        onCancel={() => agenda.cancel(booking.id)}
      />
    {:else}
      <p class="muted">No quedan turnos por atender este día.</p>
    {/if}
    <dl class="counts">
      <div><dt>turnos</dt><dd>{agenda.counts.total}</dd></div>
      <div><dt>atendidos</dt><dd>{agenda.counts.attended}</dd></div>
      <div><dt>no vino</dt><dd>{agenda.counts.noShow}</dd></div>
    </dl>
  </aside>
</div>

<style>
  .layout {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-5);
    align-items: flex-start;
  }

  .main {
    flex: 999 1 36rem;
    min-width: 0;
  }

  .side {
    flex: 1 1 16rem;
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .sheet {
    padding: var(--space-5);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
  }

  .muted {
    margin: 0;
    color: var(--color-ink-soft);
  }

  .counts {
    margin: 0;
    padding-top: var(--space-3);
    border-top: var(--line-bold) solid var(--color-ink);
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: var(--space-2);
  }

  .counts div {
    display: flex;
    flex-direction: column-reverse;
  }

  dt {
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  dd {
    margin: 0;
    font-family: var(--font-mono);
    font-size: var(--text-xl);
    font-weight: 600;
  }
</style>
