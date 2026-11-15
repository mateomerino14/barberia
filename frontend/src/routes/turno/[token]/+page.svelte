<!-- Turno del cliente: el talón, agregar al calendario, cambiar la hora y cancelar. -->
<script lang="ts">
  import {page} from '$app/state';
  import {onMount} from 'svelte';
  import {icsHref} from '#lib/core/calendar.ts';
  import {clockTime, dateOf} from '#lib/core/time.ts';
  import {whatsappHref} from '#lib/core/whatsapp.ts';
  import {TicketState} from '#lib/features/booking/ticket.svelte.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';
  import SlotGrid from '#lib/ui/organisms/SlotGrid.svelte';
  import Ticket from '#lib/ui/organisms/Ticket.svelte';

  const ticket = new TicketState(page.params.token ?? '');

  onMount(() => {
    ticket.load();
  });

  /** Enlace para guardar el turno en el calendario del teléfono. */
  function calendarLink(): string {
    const booking = ticket.booking;
    if (!booking) {
      return '';
    }
    return icsHref({
      id: String(booking.number),
      title: `${booking.serviceName} en ${ticket.shop?.name ?? 'la barbería'}`,
      location: ticket.shop?.address ?? '',
      start: booking.start,
      end: booking.end,
    });
  }

  /** Enlace para escribirle a la barbería por WhatsApp. */
  function contactLink(): string {
    const booking = ticket.booking;
    const phone = ticket.shop?.phone ?? '';
    if (!booking || !phone) {
      return '';
    }
    return whatsappHref(phone, `Hola, tengo el turno N.º ${booking.number} a las ${clockTime(booking.start)}.`);
  }

  let confirming = $state(false);

  /** Cancela el turno después de que el cliente lo confirma. */
  async function cancel(): Promise<void> {
    await ticket.cancel();
    confirming = false;
  }
</script>

<svelte:head>
  <title>Tu turno · {ticket.shop?.name ?? 'Barbería'}</title>
</svelte:head>

<main class="page">
  {#if ticket.loading}
    <p class="muted">Buscando tu turno…</p>
  {:else if !ticket.booking}
    <h1 class="title">No encontramos ese turno</h1>
    <p class="muted">Revisa que el enlace esté completo, o reserva uno nuevo.</p>
    <a class="link" href="/">Reservar un turno</a>
  {:else}
    <div class="intro">
      <span class="kicker">{ticket.shop?.name ?? 'Barbería'}</span>
      {#if ticket.booking.status === 'Cancelled'}
        <h1 class="title">Turno cancelado</h1>
      {:else}
        <h1 class="title">Listo, {ticket.booking.customerName}. Te esperamos.</h1>
      {/if}
    </div>

    {#if ticket.error}
      <Notice>{ticket.error}</Notice>
    {/if}
    {#if ticket.notice}
      <Notice tone="success">{ticket.notice}</Notice>
    {/if}

    <Ticket ticket={ticket.booking} cancelled={ticket.booking.status === 'Cancelled'} />

    {#if ticket.shop?.address}
      <p class="address"><strong>{ticket.shop.address}</strong></p>
    {/if}
    <p class="muted">Guarda este enlace: con él puedes ver, cambiar o cancelar tu turno.</p>

    {#if ticket.booking.status === 'Booked'}
      <div class="actions">
        <a class="primary-link" href={calendarLink()} download="turno.ics">Agregar a mi calendario</a>
        {#if ticket.canChange}
          <div class="pair">
            <Button variant="secondary" wide onclick={() => ticket.startChange(dateOf(ticket.booking?.start ?? ''))}>
              Cambiar hora
            </Button>
            <Button variant="danger" wide disabled={ticket.busy} onclick={() => (confirming = true)}>Cancelar</Button>
          </div>
          {#if confirming}
            <div class="confirm" role="alertdialog" aria-label="Confirmar cancelación">
              <p>¿Seguro que quieres cancelar tu turno? La hora queda libre para otra persona.</p>
              <div class="pair">
                <Button wide disabled={ticket.busy} onclick={cancel}>Sí, cancelar</Button>
                <Button variant="secondary" wide onclick={() => (confirming = false)}>No, mantener</Button>
              </div>
            </div>
          {/if}
        {:else}
          <p class="muted">Ya pasó el plazo para cambiar por aquí. Si necesitas algo, escríbele a la barbería.</p>
        {/if}
        {#if contactLink()}
          <a class="link" href={contactLink()} target="_blank" rel="noopener">Escribir a la barbería por WhatsApp</a>
        {/if}
      </div>
    {/if}

    {#if ticket.changing}
      <section class="change" aria-label="Cambiar la hora">
        <FormField label="Día" id="change-date">
          <Input
            id="change-date"
            type="date"
            mono
            value={ticket.date}
            onchange={(event) => ticket.loadSlots(event.currentTarget.value)}
          />
        </FormField>
        <SlotGrid
          slots={ticket.slots}
          selected={null}
          emptyText="No quedan horas libres con tu barbero ese día."
          onSelect={(start) => ticket.moveTo(start)}
        />
      </section>
    {/if}
  {/if}
</main>

<style>
  .page {
    max-width: 30rem;
    margin: 0 auto;
    padding: var(--space-6) var(--space-5);
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .intro {
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .kicker {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: var(--color-ink-soft);
  }

  .title {
    margin: 0;
    font-size: var(--text-display);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 0.95;
    text-transform: uppercase;
  }

  .address,
  .muted {
    margin: 0;
  }

  .muted {
    color: var(--color-ink-soft);
  }

  .actions {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .primary-link {
    min-height: var(--touch-min);
    display: grid;
    place-items: center;
    background: var(--color-ink);
    color: var(--color-sheet);
    font-size: var(--text-lg);
    font-weight: 800;
    font-stretch: var(--stretch-button);
    letter-spacing: 0.04em;
    text-decoration: none;
    text-transform: uppercase;
  }

  .pair {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: var(--space-2);
  }


  .link {
    color: var(--color-action);
    font-weight: 600;
  }

  .confirm {
    padding: var(--space-4);
    border: var(--line-bold) solid var(--color-action);
    background: var(--color-sheet);
  }

  .confirm p {
    margin: 0 0 var(--space-3);
  }

  .change {
    padding: var(--space-4);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }
</style>
