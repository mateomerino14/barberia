<!-- Formulario de un servicio: nombre, duración, precio y si se muestra a los clientes. -->
<script lang="ts">
  import {parsePrice, priceInput} from '#lib/core/money.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Checkbox from '#lib/ui/atoms/Checkbox.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';

  interface ServiceValues {
    name: string;
    durationMinutes: number;
    priceCents: number;
    isActive: boolean;
  }

  interface Props {
    initial?: ServiceValues | null;
    saving?: boolean;
    onSubmit: (values: ServiceValues) => void;
    onCancel: () => void;
  }

  const {initial = null, saving = false, onSubmit, onCancel}: Props = $props();

  const durationStep = 5;
  const defaultDuration = 30;
  const nameMaxLength = 80;

  let name = $state('');
  let duration = $state(String(defaultDuration));
  let price = $state('');
  let isActive = $state(true);
  let priceError = $state<string | null>(null);

  $effect(() => {
    if (initial) {
      name = initial.name;
      duration = String(initial.durationMinutes);
      price = priceInput(initial.priceCents);
      isActive = initial.isActive;
    }
  });

  /** Valida el precio y envía los datos. */
  function submit(event: SubmitEvent): void {
    event.preventDefault();
    const cents = parsePrice(price);
    if (cents === null || cents < 0) {
      priceError = 'Escribe el precio en bolivianos, por ejemplo 50 o 52,50.';
      return;
    }
    priceError = null;
    onSubmit({name, durationMinutes: Number(duration), priceCents: cents, isActive});
  }
</script>

<form class="form" onsubmit={submit}>
  <FormField label="Nombre" id="service-name">
    <Input id="service-name" required maxlength={nameMaxLength} placeholder="Degradado" bind:value={name} />
  </FormField>
  <div class="pair">
    <FormField label="Duración (minutos)" id="service-duration" hint="En pasos de 5 minutos">
      <Input
        id="service-duration"
        type="number"
        mono
        required
        min={durationStep}
        step={durationStep}
        bind:value={duration}
      />
    </FormField>
    <FormField label="Precio (Bs)" id="service-price" error={priceError}>
      <Input
        id="service-price"
        inputmode="decimal"
        mono
        required
        placeholder="50"
        invalid={priceError !== null}
        bind:value={price}
      />
    </FormField>
  </div>
  <Checkbox bind:checked={isActive}>Se muestra a los clientes</Checkbox>
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

  .pair {
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
