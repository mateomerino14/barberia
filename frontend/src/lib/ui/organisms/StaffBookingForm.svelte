<!-- Formulario para anotar un turno desde el panel: por teléfono o alguien que llegó sin reserva. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import Checkbox from '#lib/ui/atoms/Checkbox.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Select from '#lib/ui/atoms/Select.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';

  interface Option {
    id: string;
    name: string;
  }

  interface Values {
    serviceId: string;
    barberId: string;
    time: string;
    customerName: string;
    customerPhone: string;
    isWalkIn: boolean;
  }

  interface Props {
    services: Option[];
    barbers: Option[];
    defaultTime: string;
    busy?: boolean;
    onSubmit: (values: Values) => void;
    onCancel: () => void;
  }

  const {services, barbers, defaultTime, busy = false, onSubmit, onCancel}: Props = $props();
  const nameMaxLength = 80;

  let serviceId = $state('');
  let barberId = $state('');
  let time = $state('');
  let customerName = $state('');
  let customerPhone = $state('');
  let isWalkIn = $state(true);

  $effect(() => {
    serviceId = services[0]?.id ?? '';
    barberId = barbers[0]?.id ?? '';
    time = defaultTime;
  });

  /** Convierte la lista en opciones de la lista desplegable. */
  function toOptions(list: Option[]) {
    return list.map((item) => ({value: item.id, label: item.name}));
  }

  /** Envía el turno nuevo. */
  function submit(event: SubmitEvent): void {
    event.preventDefault();
    onSubmit({serviceId, barberId, time, customerName, customerPhone, isWalkIn});
  }
</script>

<form class="form" onsubmit={submit}>
  <div class="grid">
    <FormField label="Servicio" id="staff-service">
      <Select id="staff-service" options={toOptions(services)} required bind:value={serviceId} />
    </FormField>
    <FormField label="Barbero" id="staff-barber">
      <Select id="staff-barber" options={toOptions(barbers)} required bind:value={barberId} />
    </FormField>
    <FormField label="Hora" id="staff-time">
      <Input id="staff-time" type="time" mono required bind:value={time} />
    </FormField>
    <FormField label="Nombre" id="staff-name">
      <Input id="staff-name" required maxlength={nameMaxLength} bind:value={customerName} />
    </FormField>
    <FormField label="Celular" id="staff-phone">
      <Input id="staff-phone" type="tel" mono required={!isWalkIn} bind:value={customerPhone} />
    </FormField>
  </div>
  <Checkbox bind:checked={isWalkIn}>Llegó sin reserva (el celular es opcional)</Checkbox>
  <div class="actions">
    <Button type="submit" disabled={busy}>Anotar</Button>
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
    grid-template-columns: repeat(auto-fit, minmax(11rem, 1fr));
    gap: var(--space-3);
  }


  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
  }
</style>
