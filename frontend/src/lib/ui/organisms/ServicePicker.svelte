<!-- Lista de servicios para elegir uno, con duración y precio. -->
<script lang="ts">
  import {formatPrice} from '#lib/core/money.ts';
  import {formatDuration} from '#lib/core/time.ts';

  interface ServiceOption {
    id: string;
    name: string;
    durationMinutes: number;
    priceCents: number;
  }

  interface Props {
    services: ServiceOption[];
    selectedId: string | null;
    onSelect: (id: string) => void;
  }

  const {services, selectedId, onSelect}: Props = $props();
</script>

<div class="list" role="radiogroup" aria-label="Servicios">
  {#each services as service (service.id)}
    <label class="option" class:selected={service.id === selectedId}>
      <input
        type="radio"
        name="service"
        value={service.id}
        checked={service.id === selectedId}
        onchange={() => onSelect(service.id)}
      />
      <span class="text">
        <strong class="name">{service.name}</strong>
        <span class="duration">{formatDuration(service.durationMinutes)}</span>
      </span>
      <span class="price">{formatPrice(service.priceCents)}</span>
    </label>
  {:else}
    <p class="empty">La barbería todavía no publicó sus servicios.</p>
  {/each}
</div>

<style>
  .list {
    display: flex;
    flex-direction: column;
    border-top: var(--line-thin) solid var(--color-ink);
  }

  .option {
    min-height: var(--touch-min);
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-1);
    border-bottom: var(--line-thin) solid var(--color-rule);
    cursor: pointer;
  }

  .selected {
    background: var(--color-sheet);
  }

  input {
    width: 1.25rem;
    height: 1.25rem;
    accent-color: var(--color-selected);
  }

  .text {
    flex: 1;
    display: flex;
    flex-direction: column;
  }

  .name {
    font-size: var(--text-md);
  }

  .duration,
  .price {
    font-family: var(--font-mono);
  }

  .duration {
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  .empty {
    margin: 0;
    padding: var(--space-3) 0;
    color: var(--color-ink-soft);
  }
</style>
