<!-- Formulario para bloquear horas de un barbero, como el almuerzo. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Select from '#lib/ui/atoms/Select.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';

  interface Option {
    id: string;
    name: string;
  }

  interface Values {
    barberId: string;
    from: string;
    to: string;
    reason: string;
  }

  interface Props {
    barbers: Option[];
    busy?: boolean;
    onSubmit: (values: Values) => void;
    onCancel: () => void;
  }

  const {barbers, busy = false, onSubmit, onCancel}: Props = $props();
  const reasonMaxLength = 120;

  let barberId = $derived(barbers[0]?.id ?? '');
  let from = $state('13:00');
  let to = $state('14:00');
  let reason = $state('Almuerzo');

  /** Convierte la lista en opciones de la lista desplegable. */
  function toOptions(list: Option[]) {
    return list.map((item) => ({value: item.id, label: item.name}));
  }

  /** Envía el bloqueo. */
  function submit(event: SubmitEvent): void {
    event.preventDefault();
    onSubmit({barberId, from, to, reason});
  }
</script>

<form class="form" onsubmit={submit}>
  <div class="grid">
    <FormField label="Barbero" id="block-barber">
      <Select id="block-barber" options={toOptions(barbers)} required bind:value={barberId} />
    </FormField>
    <FormField label="Desde" id="block-from">
      <Input id="block-from" type="time" mono required bind:value={from} />
    </FormField>
    <FormField label="Hasta" id="block-to">
      <Input id="block-to" type="time" mono required bind:value={to} />
    </FormField>
    <FormField label="Motivo" id="block-reason">
      <Input id="block-reason" maxlength={reasonMaxLength} bind:value={reason} />
    </FormField>
  </div>
  <div class="actions">
    <Button type="submit" disabled={busy}>Bloquear</Button>
    <Button variant="secondary" onclick={onCancel}>Cerrar</Button>
  </div>
</form>

<style>
  .form {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(10rem, 1fr));
    gap: var(--space-3);
  }


  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
  }
</style>
