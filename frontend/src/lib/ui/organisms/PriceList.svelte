<!-- Lista de precios con puntos guía, como la pizarra de la barbería. -->
<script lang="ts">
  import {formatPrice} from '#lib/core/money.ts';
  import {formatDuration} from '#lib/core/time.ts';
  interface PriceItem {
    id: string;
    name: string;
    durationMinutes: number;
    priceCents: number;
  }

  interface Props {
    services: PriceItem[];
  }

  const {services}: Props = $props();
</script>

<ul class="prices">
  {#each services as service (service.id)}
    <li class="row">
      <span class="name">{service.name}</span>
      <span class="leader" aria-hidden="true"></span>
      <span class="duration">{formatDuration(service.durationMinutes)}</span>
      <span class="price">{formatPrice(service.priceCents)}</span>
    </li>
  {/each}
</ul>

<style>
  .prices {
    margin: 0;
    padding: 0;
    list-style: none;
    display: flex;
    flex-direction: column;
  }

  .row {
    display: flex;
    align-items: baseline;
    gap: var(--space-3);
    padding: var(--space-3) 0;
    border-bottom: var(--line-thin) solid var(--color-rule);
  }

  .name {
    font-size: var(--text-lg);
    font-weight: 700;
  }

  .leader {
    flex: 1;
    min-width: var(--space-4);
    border-bottom: var(--line-bold) dotted var(--color-rule);
  }

  .duration {
    font-family: var(--font-mono);
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  .price {
    font-family: var(--font-mono);
    font-size: var(--text-lg);
    font-weight: 600;
  }
</style>
