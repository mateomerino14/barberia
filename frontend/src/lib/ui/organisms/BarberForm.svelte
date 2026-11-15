<!-- Formulario de un barbero: nombre, servicios que hace, si recibe turnos y, al crearlo, su cuenta. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import Checkbox from '#lib/ui/atoms/Checkbox.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';

  interface ServiceOption {
    id: string;
    name: string;
  }

  interface BarberValues {
    name: string;
    isActive: boolean;
    serviceIds: string[];
    account: {email: string; password: string} | null;
  }

  interface Props {
    services: ServiceOption[];
    initial?: {name: string; isActive: boolean; serviceIds: string[]} | null;
    withAccount?: boolean;
    saving?: boolean;
    onSubmit: (values: BarberValues) => void;
    onCancel: () => void;
  }

  const {services, initial = null, withAccount = false, saving = false, onSubmit, onCancel}: Props = $props();

  const nameMaxLength = 80;
  const passwordMinLength = 8;

  let name = $state('');
  let isActive = $state(true);
  let selected = $state<Record<string, boolean>>({});
  let createAccount = $state(false);
  let email = $state('');
  let password = $state('');

  $effect(() => {
    if (initial) {
      name = initial.name;
      isActive = initial.isActive;
      selected = Object.fromEntries(initial.serviceIds.map((id) => [id, true]));
    }
  });

  /** Envía los datos del barbero y, si se pidió, los de su cuenta. */
  function submit(event: SubmitEvent): void {
    event.preventDefault();
    let account = null;
    if (withAccount && createAccount) {
      account = {email, password};
    }
    const serviceIds = services.filter((service) => selected[service.id]).map((service) => service.id);
    onSubmit({name, isActive, serviceIds, account});
  }
</script>

<form class="form" onsubmit={submit}>
  <FormField label="Nombre" id="barber-name">
    <Input id="barber-name" required maxlength={nameMaxLength} placeholder="Lucho" bind:value={name} />
  </FormField>
  <fieldset class="services">
    <legend>Servicios que hace</legend>
    {#if services.length === 0}
      <p class="empty">Primero agrega servicios en la página de servicios.</p>
    {/if}
    {#each services as service (service.id)}
      <Checkbox bind:checked={selected[service.id]}>{service.name}</Checkbox>
    {/each}
  </fieldset>
  <Checkbox bind:checked={isActive}>Recibe turnos</Checkbox>
  {#if withAccount}
    <Checkbox bind:checked={createAccount}>Darle acceso al panel para ver su agenda</Checkbox>
    {#if createAccount}
      <div class="pair">
        <FormField label="Correo" id="barber-email">
          <Input id="barber-email" type="email" required autocomplete="off" bind:value={email} />
        </FormField>
        <FormField label="Contraseña" id="barber-password" hint="Al menos 8 caracteres">
          <Input
            id="barber-password"
            type="password"
            required
            minlength={passwordMinLength}
            autocomplete="new-password"
            bind:value={password}
          />
        </FormField>
      </div>
    {/if}
  {/if}
  <div class="actions">
    <Button type="submit" disabled={saving}>Guardar</Button>
    <Button variant="secondary" onclick={onCancel}>Cancelar</Button>
  </div>
</form>

<style>
  .form {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .services {
    margin: 0;
    padding: var(--space-3) var(--space-4);
    border: var(--line-thin) solid var(--color-rule);
    display: flex;
    flex-wrap: wrap;
    gap: 0 var(--space-5);
  }

  legend {
    padding: 0 var(--space-2);
    font-size: var(--text-sm);
    font-weight: 700;
  }

  .empty {
    margin: 0;
    color: var(--color-ink-soft);
  }

  .pair {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(12rem, 1fr));
    gap: var(--space-3);
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
  }
</style>
