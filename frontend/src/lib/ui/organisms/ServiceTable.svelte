<!-- Lista de servicios del panel con su duración, precio, estado y botón para editar. -->
<script lang="ts">
  import {formatPrice} from '#lib/core/money.ts';
  import {formatDuration} from '#lib/core/time.ts';
  import Badge from '#lib/ui/atoms/Badge.svelte';

  interface ServiceRow {
    id: string;
    name: string;
    durationMinutes: number;
    priceCents: number;
    isActive: boolean;
  }

  interface Props {
    services: ServiceRow[];
    onEdit: (id: string) => void;
  }

  const {services, onEdit}: Props = $props();
</script>

{#if services.length === 0}
  <p class="empty">Todavía no hay servicios. Agrega el primero, por ejemplo "Corte clásico".</p>
{:else}
  <ul class="table">
    {#each services as service (service.id)}
      <li class="row" class:hidden={!service.isActive}>
        <div class="main">
          <strong class="name">{service.name}</strong>
          <span class="meta">{formatDuration(service.durationMinutes)}</span>
        </div>
        {#if !service.isActive}
          <Badge>Oculto</Badge>
        {/if}
        <span class="price">{formatPrice(service.priceCents)}</span>
        <button
          class="edit"
          type="button"
          onclick={() => onEdit(service.id)}
          aria-label="Editar {service.name}"
        >
          Editar</button>
      </li>
    {/each}
  </ul>
{/if}

<style>
  .empty {
    margin: 0;
    color: var(--color-ink-soft);
  }

  .table {
    margin: 0;
    padding: 0;
    list-style: none;
    border-top: var(--line-thin) solid var(--color-ink);
  }

  .row {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-1);
    border-bottom: var(--line-thin) solid var(--color-rule);
  }

  .hidden .name {
    color: var(--color-ink-soft);
  }

  .main {
    display: flex;
    flex-direction: column;
    flex: 1;
  }

  .meta,
  .price {
    font-family: var(--font-mono);
  }

  .meta {
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  .edit {
    min-height: var(--touch-min);
    padding: 0 var(--space-4);
    border: var(--line-firm) solid var(--color-ink);
    background: transparent;
    font-family: var(--font-sans);
    font-weight: 700;
    cursor: pointer;
  }
</style>
