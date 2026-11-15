<!-- Datos de la barbería y reglas de reserva. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import {ShopState} from '#lib/features/shop/state.svelte.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';

  const settings = new ShopState();
  const nameMaxLength = 80;
  const addressMaxLength = 160;
  const phoneMaxLength = 20;
  const welcomeMaxLength = 280;
  const maxDaysLimit = 90;
  const maxLeadMinutes = 1440;
  const maxCancelMinutes = 2880;

  let name = $state('');
  let address = $state('');
  let phone = $state('');
  let welcomeMessage = $state('');
  let minLead = $state('60');
  let maxDays = $state('14');
  let cancelLimit = $state('120');

  onMount(() => {
    settings.load().then(() => {
      if (settings.shop) {
        name = settings.shop.name;
        address = settings.shop.address;
        phone = settings.shop.phone;
        welcomeMessage = settings.shop.welcomeMessage;
        minLead = String(settings.shop.rules.minLeadMinutes);
        maxDays = String(settings.shop.rules.maxDaysAhead);
        cancelLimit = String(settings.shop.rules.cancelLimitMinutes);
      }
    });
  });

  /** Guarda los datos y las reglas. */
  function submit(event: SubmitEvent): void {
    event.preventDefault();
    settings.saveProfile({
      name,
      address,
      phone,
      welcomeMessage,
      rules: {minLeadMinutes: Number(minLead), maxDaysAhead: Number(maxDays), cancelLimitMinutes: Number(cancelLimit)},
    });
  }
</script>

<svelte:head>
  <title>Ajustes · Barbería</title>
</svelte:head>

<PageTitle kicker="La barbería" title="Ajustes" />

{#if settings.error}
  <Notice>{settings.error}</Notice>
{/if}
{#if settings.notice}
  <Notice tone="success">{settings.notice}</Notice>
{/if}

<form class="form" onsubmit={submit}>
  <fieldset class="group">
    <legend>Lo que ve el cliente</legend>
    <FormField label="Nombre de la barbería" id="shop-name">
      <Input id="shop-name" required maxlength={nameMaxLength} bind:value={name} />
    </FormField>
    <FormField label="Dirección" id="shop-address">
      <Input id="shop-address" maxlength={addressMaxLength} bind:value={address} />
    </FormField>
    <FormField label="Teléfono o WhatsApp" id="shop-phone">
      <Input id="shop-phone" type="tel" mono maxlength={phoneMaxLength} bind:value={phone} />
    </FormField>
    <FormField label="Mensaje de bienvenida" id="shop-welcome" hint="Una línea que aparece al reservar">
      <Input id="shop-welcome" maxlength={welcomeMaxLength} bind:value={welcomeMessage} />
    </FormField>
  </fieldset>
  <fieldset class="group">
    <legend>Reglas de reserva</legend>
    <FormField label="Anticipación mínima (minutos)" id="rules-lead" hint="Cuánto antes como mínimo se puede reservar">
      <Input id="rules-lead" type="number" mono min="0" max={maxLeadMinutes} required bind:value={minLead} />
    </FormField>
    <FormField label="Días hacia adelante" id="rules-days" hint="Hasta cuántos días adelante se puede reservar">
      <Input id="rules-days" type="number" mono min="1" max={maxDaysLimit} required bind:value={maxDays} />
    </FormField>
    <FormField
      label="Plazo para cancelar (minutos)"
      id="rules-cancel"
      hint="Hasta cuánto antes el cliente puede cancelar o cambiar"
    >
      <Input id="rules-cancel" type="number" mono min="0" max={maxCancelMinutes} required bind:value={cancelLimit} />
    </FormField>
  </fieldset>
  <div>
    <Button type="submit" disabled={settings.saving}>Guardar ajustes</Button>
  </div>
</form>

<style>
  .form {
    display: flex;
    flex-direction: column;
    gap: var(--space-5);
    max-width: 40rem;
  }

  .group {
    margin: 0;
    padding: var(--space-4);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  legend {
    padding: 0 var(--space-2);
    font-weight: 800;
    font-stretch: var(--stretch-button);
    text-transform: uppercase;
  }
</style>
