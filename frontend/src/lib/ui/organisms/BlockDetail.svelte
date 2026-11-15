<!-- Ficha de un bloqueo elegido en la agenda, con el botón para quitarlo. -->
<script lang="ts">
  import {clockTime} from '#lib/core/time.ts';
  import Button from '#lib/ui/atoms/Button.svelte';

  interface Props {
    barberName: string;
    start: string;
    end: string;
    reason: string;
    busy?: boolean;
    onRemove: () => void;
    onClose: () => void;
  }

  const {barberName, start, end, reason, busy = false, onRemove, onClose}: Props = $props();
</script>

<section class="card" aria-label="Bloqueo elegido">
  <h2 class="heading">Horas bloqueadas</h2>
  <strong class="when">{clockTime(start)} – {clockTime(end)}</strong>
  <span>{reason || 'Sin motivo'} · {barberName}</span>
  <p class="help">En estas horas nadie puede reservar con {barberName}.</p>
  <div class="actions">
    <Button variant="danger" wide disabled={busy} onclick={onRemove}>Quitar bloqueo</Button>
    <Button variant="secondary" wide onclick={onClose}>Cerrar</Button>
  </div>
</section>

<style>
  .card {
    padding: var(--space-4);
    border: var(--line-thin) dashed var(--color-ink);
    background: var(--color-sheet);
    display: flex;
    flex-direction: column;
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

  .when {
    font-family: var(--font-mono);
    font-size: var(--text-xl);
  }

  .help {
    margin: 0;
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  .actions {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }
</style>
