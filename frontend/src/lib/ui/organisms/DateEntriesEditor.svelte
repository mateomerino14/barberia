<!-- Lista de fechas con motivo (días cerrados o ausencias), con su formulario para agregar una más. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';

  interface DateEntry {
    key: string;
    label: string;
    reason: string;
  }

  interface Props {
    idPrefix: string;
    entries: DateEntry[];
    range?: boolean;
    emptyText: string;
    reasonPlaceholder?: string;
    busy?: boolean;
    onAdd: (from: string, to: string, reason: string) => void;
    onRemove: (key: string) => void;
  }

  const {idPrefix, entries, range = false, emptyText, reasonPlaceholder = '', busy = false, onAdd, onRemove}: Props = $props();

  const reasonMaxLength = 120;

  let from = $state('');
  let to = $state('');
  let reason = $state('');

  /** Etiqueta del primer campo de fecha según si se pide un rango. */
  function fromLabel(): string {
    if (range) {
      return 'Desde';
    }
    return 'Fecha';
  }

  /** Envía la fecha nueva y limpia el formulario. */
  function add(event: SubmitEvent): void {
    event.preventDefault();
    let end = from;
    if (range && to) {
      end = to;
    }
    onAdd(from, end, reason);
    from = '';
    to = '';
    reason = '';
  }
</script>

<div class="entries">
  {#if entries.length === 0}
    <p class="empty">{emptyText}</p>
  {:else}
    <ul class="list">
      {#each entries as entry (entry.key)}
        <li class="item">
          <span class="date">{entry.label}</span>
          <span class="reason">{entry.reason}</span>
          <button class="remove" type="button" disabled={busy} onclick={() => onRemove(entry.key)}>Quitar</button>
        </li>
      {/each}
    </ul>
  {/if}
  <form class="add" onsubmit={add}>
    <FormField label={fromLabel()} id="{idPrefix}-from">
      <Input id="{idPrefix}-from" type="date" mono required bind:value={from} />
    </FormField>
    {#if range}
      <FormField label="Hasta" id="{idPrefix}-to" hint="Vacío si es un solo día">
        <Input id="{idPrefix}-to" type="date" mono min={from} bind:value={to} />
      </FormField>
    {/if}
    <FormField label="Motivo" id="{idPrefix}-reason">
      <Input id="{idPrefix}-reason" maxlength={reasonMaxLength} placeholder={reasonPlaceholder} bind:value={reason} />
    </FormField>
    <div class="submit">
      <Button type="submit" variant="secondary" disabled={busy || !from}>Agregar</Button>
    </div>
  </form>
</div>

<style>
  .entries {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .empty {
    margin: 0;
    color: var(--color-ink-soft);
  }

  .list {
    margin: 0;
    padding: 0;
    list-style: none;
    border-top: var(--line-thin) solid var(--color-ink);
  }

  .item {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-2) 0;
    border-bottom: var(--line-thin) solid var(--color-rule);
  }

  .date {
    font-family: var(--font-mono);
  }

  .reason {
    flex: 1;
    color: var(--color-ink-soft);
  }

  .remove {
    min-height: var(--touch-min);
    padding: 0 var(--space-3);
    border: var(--line-thin) solid var(--color-action);
    background: transparent;
    color: var(--color-action);
    font-family: var(--font-sans);
    cursor: pointer;
  }

  .add {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(10rem, 1fr));
    gap: var(--space-3);
    align-items: end;
  }

  .submit {
    padding-bottom: var(--space-1);
  }
</style>
