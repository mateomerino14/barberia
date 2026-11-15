<!-- Ficha del turno elegido o del siguiente: quién viene, recordatorio por WhatsApp y cómo terminó. -->
<script lang="ts">
  import {formatPrice} from '#lib/core/money.ts';
  import {clockTime} from '#lib/core/time.ts';
  import Badge from '#lib/ui/atoms/Badge.svelte';

  interface Detail {
    number: number;
    customerName: string;
    customerPhone: string;
    serviceName: string;
    barberName: string;
    start: string;
    priceCents: number;
    status: 'Booked' | 'Attended' | 'NoShow';
    isWalkIn: boolean;
  }

  interface Props {
    heading: string;
    detail: Detail;
    whatsappLink: string;
    busy?: boolean;
    onAttended: () => void;
    onNoShow: () => void;
    onUndo: () => void;
    onCancel: () => void;
  }

  const {heading, detail, whatsappLink, busy = false, onAttended, onNoShow, onUndo, onCancel}: Props = $props();
</script>

<section class="card" aria-label={heading}>
  <h2 class="heading">{heading}</h2>
  <strong class="who">{detail.customerName} · {clockTime(detail.start)}</strong>
  <span>{detail.serviceName} con {detail.barberName}</span>
  <span class="mono">{formatPrice(detail.priceCents)}{#if detail.customerPhone} · {detail.customerPhone}{/if}</span>
  {#if detail.isWalkIn}
    <Badge>Sin reserva</Badge>
  {/if}
  {#if whatsappLink}
    <a class="whatsapp" href={whatsappLink} target="_blank" rel="noopener">Recordarle por WhatsApp</a>
  {/if}
  {#if detail.status === 'Booked'}
    <div class="pair">
      <button class="done" type="button" disabled={busy} onclick={onAttended}>Atendido</button>
      <button class="missed" type="button" disabled={busy} onclick={onNoShow}>No vino</button>
    </div>
    <button class="text" type="button" disabled={busy} onclick={onCancel}>Cancelar turno</button>
  {:else}
    <Badge tone="selected">{#if detail.status === 'Attended'}Atendido{:else}No vino{/if}</Badge>
    <button class="text" type="button" disabled={busy} onclick={onUndo}>Deshacer</button>
  {/if}
</section>

<style>
  .card {
    padding: var(--space-4);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: var(--space-2);
  }

  .heading {
    margin: 0;
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    font-weight: 400;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: var(--color-ink-soft);
  }

  .who {
    font-size: var(--text-xl);
    font-stretch: var(--stretch-button);
    text-transform: uppercase;
  }

  .mono {
    font-family: var(--font-mono);
    font-size: var(--text-sm);
  }

  .whatsapp {
    align-self: stretch;
    min-height: var(--touch-min);
    display: grid;
    place-items: center;
    border: var(--line-firm) solid var(--color-ink);
    color: var(--color-ink);
    font-weight: 700;
    text-decoration: none;
  }

  .pair {
    align-self: stretch;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: var(--space-2);
  }

  .done,
  .missed {
    min-height: var(--touch-min);
    font-family: var(--font-sans);
    font-size: var(--text-md);
    font-weight: 700;
    cursor: pointer;
  }

  .done {
    border: 0;
    background: var(--color-ink);
    color: var(--color-sheet);
  }

  .missed {
    border: var(--line-thin) solid var(--color-action);
    background: transparent;
    color: var(--color-action);
  }

  .text {
    min-height: var(--touch-min);
    padding: 0;
    border: 0;
    background: none;
    color: var(--color-action);
    font-family: var(--font-sans);
    text-decoration: underline;
    cursor: pointer;
  }
</style>
