<!-- Título de un paso de la reserva con su número en rojo, como en el cuaderno de turnos. -->
<script lang="ts">
  interface Props {
    step: string;
    title: string;
  }

  const {step, title}: Props = $props();
</script>

<h2 class="step-title"><span class="number">{step}</span>{title}</h2>

<style>
  .step-title {
    margin: 0;
    display: flex;
    align-items: baseline;
    gap: var(--space-3);
    font-size: var(--text-xl);
    font-weight: 800;
    font-stretch: var(--stretch-button);
    text-transform: uppercase;
  }

  .number {
    font-family: var(--font-mono);
    font-size: var(--text-sm);
    font-weight: 400;
    color: var(--color-action);
  }
</style>
