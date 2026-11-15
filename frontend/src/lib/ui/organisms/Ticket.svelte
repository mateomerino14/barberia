<!-- El turno como talón de ticket: número, servicio, barbero, día, precio y hora. -->
<script lang="ts">
  import {formatPrice} from '#lib/core/money.ts';
  import {clockTime, dateOf, formatLongDate} from '#lib/core/time.ts';
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';

  interface TicketData {
    number: number;
    serviceName: string;
    barberName: string;
    start: string;
    priceCents: number;
  }

  interface Props {
    ticket: TicketData;
    cancelled?: boolean;
    footnote?: string;
  }

  const {ticket, cancelled = false, footnote = 'Llega 5 minutos antes'}: Props = $props();
  const numberDigits = 4;
</script>

<article class="ticket" class:cancelled aria-label="Turno {ticket.number}">
  <PoleStripe />
  <div class="head">
    <span class="label">TURNO N.º</span>
    <span class="number">{String(ticket.number).padStart(numberDigits, '0')}</span>
  </div>
  <dl class="data">
    <dt>Servicio</dt>
    <dd>{ticket.serviceName}</dd>
    <dt>Barbero</dt>
    <dd>{ticket.barberName}</dd>
    <dt>Día</dt>
    <dd class="date">{formatLongDate(dateOf(ticket.start))}</dd>
    <dt>Precio</dt>
    <dd class="mono">{formatPrice(ticket.priceCents)}, en la barbería</dd>
  </dl>
  <div class="stub">
    {#if cancelled}
      <span class="note">Cancelado</span>
    {:else}
      <span class="note">{footnote}</span>
    {/if}
    <span class="time">{clockTime(ticket.start)}</span>
  </div>
</article>

<style>
  .ticket {
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
  }

  .head {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    padding: var(--space-4) var(--space-4) var(--space-3);
  }

  .label {
    font-family: var(--font-mono);
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  .number {
    font-family: var(--font-mono);
    font-size: var(--text-xl);
    font-weight: 600;
  }

  .data {
    margin: 0;
    padding: 0 var(--space-4) var(--space-4);
    display: grid;
    grid-template-columns: 6rem 1fr;
    row-gap: var(--space-2);
  }

  dt {
    color: var(--color-ink-soft);
  }

  dd {
    margin: 0;
    font-weight: 700;
  }

  .date::first-letter {
    text-transform: uppercase;
  }

  .mono {
    font-family: var(--font-mono);
    font-weight: 400;
  }

  .stub {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: var(--space-4);
    border-top: var(--line-bold) dashed var(--color-rule);
  }

  .note {
    color: var(--color-ink-soft);
  }

  .time {
    font-family: var(--font-mono);
    font-size: var(--text-display);
    font-weight: 600;
    line-height: 1;
  }

  .cancelled .time,
  .cancelled .number {
    text-decoration: line-through;
    color: var(--color-disabled);
  }

  .cancelled .note {
    color: var(--color-action);
    font-weight: 700;
  }
</style>
