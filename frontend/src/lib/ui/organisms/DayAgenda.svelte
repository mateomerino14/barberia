<!-- Agenda de un día por columnas de barbero, con turnos, bloqueos y la línea de la hora actual. -->
<script lang="ts">
  import {clockTime, fromMinutes, toMinutes} from '#lib/core/time.ts';

  type Tone = 'booked' | 'attended' | 'noShow' | 'next';

  interface Column {
    id: string;
    name: string;
    opens: string | null;
    closes: string | null;
  }

  interface Item {
    id: string;
    barberId: string;
    start: string;
    end: string;
    title: string;
    subtitle: string;
    tone: Tone;
  }

  interface BlockItem {
    id: string;
    barberId: string;
    start: string;
    end: string;
    reason: string;
  }

  interface Props {
    columns: Column[];
    items: Item[];
    blocks: BlockItem[];
    nowTime: string | null;
    selectedId: string | null;
    onSelect: (id: string) => void;
    onSelectBlock: (id: string) => void;
  }

  const {columns, items, blocks, nowTime, selectedId, onSelect, onSelectBlock}: Props = $props();

  const stepMinutes = 15;
  const hourMinutes = 60;
  const labelEvery = 30;
  const defaultStart = '09:00';
  const defaultEnd = '19:00';
  const toneLabels: Record<Tone, string> = {booked: '', attended: 'Atendido', noShow: 'No vino', next: 'Siguiente'};

  /** Primer minuto que muestra la agenda: la apertura más temprana del día. */
  function firstMinute(): number {
    const opens = columns.filter((column) => column.opens).map((column) => toMinutes(clockTime(column.opens ?? '')));
    if (opens.length === 0) {
      return toMinutes(defaultStart);
    }
    return Math.min(...opens);
  }

  /** Último minuto que muestra la agenda: el cierre más tardío del día. */
  function lastMinute(): number {
    const closes = columns.filter((column) => column.closes).map((column) => toMinutes(clockTime(column.closes ?? '')));
    if (closes.length === 0) {
      return toMinutes(defaultEnd);
    }
    return Math.max(...closes);
  }

  const start = $derived(firstMinute());
  const rows = $derived(Math.max(1, Math.ceil((lastMinute() - start) / stepMinutes)));
  const marks = $derived(Array.from({length: rows}, (_, index) => start + index * stepMinutes));

  /** Fila donde empieza un instante, contando desde 1. */
  function rowOf(localIso: string): number {
    return Math.floor((toMinutes(clockTime(localIso)) - start) / stepMinutes) + 1;
  }

  /** Cuántas filas ocupa un intervalo. */
  function span(from: string, to: string): number {
    return Math.max(1, Math.ceil((toMinutes(clockTime(to)) - toMinutes(clockTime(from))) / stepMinutes));
  }

  /** Columna de un barbero, contando la columna de horas. */
  function columnOf(barberId: string): number {
    return columns.findIndex((column) => column.id === barberId) + 2;
  }

  /** Posición en la cuadrícula de un turno o bloqueo. */
  function place(barberId: string, from: string, to: string): string {
    return `grid-column: ${columnOf(barberId)}; grid-row: ${rowOf(from)} / span ${span(from, to)}`;
  }

  /** Posición de la línea de la hora actual. */
  function nowPlace(): string {
    const minutes = toMinutes(nowTime ?? defaultStart) - start;
    const row = Math.floor(minutes / stepMinutes) + 1;
    const offset = (minutes % stepMinutes) / stepMinutes;
    return `grid-row: ${row}; --offset: ${offset}`;
  }

  /** Indica si la hora actual cae dentro de la agenda mostrada. */
  function showNow(): boolean {
    if (!nowTime) {
      return false;
    }
    const minutes = toMinutes(nowTime);
    return minutes >= start && minutes < start + rows * stepMinutes;
  }
</script>

<div class="frame">
  <div class="sheet" style="--columns: {columns.length}">
    <div class="head corner"></div>
    {#each columns as column (column.id)}
      <div class="head">
        <strong class="name">{column.name}</strong>
        {#if column.opens && column.closes}
          <span class="shift">{clockTime(column.opens)} – {clockTime(column.closes)}</span>
        {:else}
          <span class="shift">No trabaja</span>
        {/if}
      </div>
    {/each}

    <div class="grid" style="--rows: {rows}">
      {#each marks as mark, index (mark)}
        <span class="hour" class:full={mark % hourMinutes === 0} style="grid-row: {index + 1}">
          {#if mark % labelEvery === 0}{fromMinutes(mark)}{/if}
        </span>
        <span
          class="line"
          class:full={mark % hourMinutes === 0}
          style="grid-row: {index + 1}"
          aria-hidden="true"
        >
          </span>
      {/each}

      {#each blocks as block (block.id)}
        <button
          class="block"
          class:selected={block.id === selectedId}
          type="button"
          style={place(block.barberId, block.start, block.end)}
          onclick={() => onSelectBlock(block.id)}
        >
          <span class="time">{clockTime(block.start)}–{clockTime(block.end)}</span>
          <span>{block.reason || 'Bloqueado'}</span>
        </button>
      {/each}

      {#each items as item (item.id)}
        <button
          class="item {item.tone}"
          class:selected={item.id === selectedId}
          type="button"
          style={place(item.barberId, item.start, item.end)}
          onclick={() => onSelect(item.id)}
        >
          <span class="top">
            <span class="time">{clockTime(item.start)}–{clockTime(item.end)}</span>
            {#if toneLabels[item.tone]}
              <span class="tag">{toneLabels[item.tone]}</span>
            {/if}
          </span>
          <strong class="title">{item.title}</strong>
          <span class="subtitle">{item.subtitle}</span>
        </button>
      {/each}

      {#if showNow()}
        <div class="now" style={nowPlace()} role="img" aria-label="Hora actual {nowTime}"></div>
      {/if}
    </div>
  </div>
</div>

<style>
  .frame {
    overflow-x: auto;
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-paper);
  }

  .sheet {
    --row: 1.75rem;
    min-width: calc(4rem + var(--columns) * 11rem);
    display: grid;
    grid-template-columns: 4rem repeat(var(--columns), minmax(11rem, 1fr));
  }

  .head {
    padding: var(--space-2) var(--space-3);
    border-bottom: var(--line-bold) solid var(--color-ink);
    border-left: var(--line-thin) solid var(--color-rule);
    background: var(--color-sheet);
    display: flex;
    flex-direction: column;
  }

  .corner {
    border-left: 0;
  }

  .name {
    font-size: var(--text-lg);
    font-stretch: var(--stretch-button);
    text-transform: uppercase;
  }

  .shift {
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  .grid {
    grid-column: 1 / -1;
    display: grid;
    grid-template-columns: subgrid;
    grid-template-rows: repeat(var(--rows), var(--row));
    padding-bottom: var(--space-2);
  }

  .hour {
    grid-column: 1;
    padding-right: var(--space-2);
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    text-align: right;
    color: var(--color-ink-soft);
    border-top: var(--line-thin) dotted var(--color-rule);
  }

  .line {
    grid-column: 2 / -1;
    border-top: var(--line-thin) dotted var(--color-rule);
  }

  .hour.full,
  .line.full {
    border-top-style: solid;
  }

  .item,
  .block {
    z-index: 1;
    margin: 1px var(--space-1);
    padding: var(--space-1) var(--space-2);
    display: flex;
    flex-direction: column;
    gap: 1px;
    overflow: hidden;
    text-align: left;
    font-family: var(--font-sans);
    font-size: var(--text-sm);
  }

  .item {
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    color: var(--color-ink);
    cursor: pointer;
  }

  .item.next {
    border-color: var(--color-selected);
    background: var(--color-selected);
    color: var(--color-sheet);
  }

  .item.attended {
    border-color: var(--color-rule);
    color: var(--color-ink-soft);
  }

  .item.noShow {
    border-color: var(--color-action);
  }

  .item.noShow .title {
    text-decoration: line-through;
  }

  .item.selected,
  .block.selected {
    outline: var(--line-bold) solid var(--color-action);
    outline-offset: 1px;
  }

  .block {
    border: var(--line-thin) dashed var(--color-disabled);
    cursor: pointer;
    background: repeating-linear-gradient(45deg, var(--color-rule) 0 6px, var(--color-paper) 6px 12px);
    color: var(--color-ink-soft);
  }


  .top {
    display: flex;
    justify-content: space-between;
    gap: var(--space-2);
  }

  .time,
  .tag {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
  }

  .tag {
    text-transform: uppercase;
  }

  .now {
    z-index: 2;
    grid-column: 2 / -1;
    align-self: start;
    height: var(--line-bold);
    margin-top: calc(var(--row) * var(--offset));
    background: var(--color-action);
  }
</style>
