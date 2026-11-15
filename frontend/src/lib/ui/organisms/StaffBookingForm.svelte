<!-- Formulario para anotar un turno desde el panel: por teléfono o alguien que llegó sin reserva. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import Checkbox from '#lib/ui/atoms/Checkbox.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
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

  /** Envía el turno nuevo. */
  function submit(event: SubmitEvent): void {
    event.preventDefault();
    onSubmit({serviceId, barberId, time, customerName, customerPhone, isWalkIn});
  }
</script>

<form class="form" onsubmit={submit}>
  <div class="grid">
    <FormField label="Servicio" id="staff-service">
      <select id="staff-service" class="select" bind:value={serviceId} required>
        {#each services as service (service.id)}
          <option value={service.id}>{service.name}</option>
        {/each}
      </select>
    </FormField>
    <FormField label="Barbero" id="staff-barber">
      <select id="staff-barber" class="select" bind:value={barberId} required>
        {#each barbers as barber (barber.id)}
          <option value={barber.id}>{barber.name}</option>
        {/each}
      </select>
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

  .select {
    min-height: var(--touch-min);
    padding: 0 var(--space-2);
    border: var(--line-thin) solid var(--color-ink);
    border-radius: 0;
    background: var(--color-sheet);
    font-family: var(--font-sans);
    font-size: var(--text-md);
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
  }
</style>
