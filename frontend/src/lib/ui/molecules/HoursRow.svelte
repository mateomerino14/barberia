<!-- Fila de horario de un día: si se atiende y de qué hora a qué hora. -->
<script lang="ts">
  import Checkbox from '#lib/ui/atoms/Checkbox.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';

  interface Props {
    dayName: string;
    open?: boolean;
    opens?: string;
    closes?: string;
  }

  let {dayName, open = $bindable(false), opens = $bindable('09:00'), closes = $bindable('19:00')}: Props = $props();
</script>

<div class="row">
  <Checkbox bind:checked={open}>{dayName}</Checkbox>
  {#if open}
    <div class="times">
      <Input type="time" mono aria-label="{dayName}, abre" bind:value={opens} />
      <span class="to">a</span>
      <Input type="time" mono aria-label="{dayName}, cierra" bind:value={closes} />
    </div>
  {:else}
    <span class="closed">Cerrado</span>
  {/if}
</div>

<style>
  .row {
    display: grid;
    grid-template-columns: 9rem 1fr;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-2) 0;
    border-bottom: var(--line-thin) solid var(--color-rule);
  }

  .times {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    max-width: 20rem;
  }

  .to {
    color: var(--color-ink-soft);
  }

  .closed {
    font-family: var(--font-mono);
    color: var(--color-ink-soft);
  }
</style>
