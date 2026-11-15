<!-- Mi barbería: los datos que ve el cliente y las reglas para reservar. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import {aheadOptions, cancelOptions, leadOptions} from '#lib/features/shop/rule-options.ts';
  import {ShopState} from '#lib/features/shop/state.svelte.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import Select from '#lib/ui/atoms/Select.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';

  const settings = new ShopState();
  const nameMaxLength = 80;
  const addressMaxLength = 160;
  const phoneMaxLength = 20;
  const welcomeMaxLength = 280;

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
  <title>Mi barbería · Panel</title>
</svelte:head>

<PageTitle kicker="Datos y reglas" title="Mi barbería" />

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
    <FormField label="¿Con cuánta anticipación se puede reservar?" id="rules-lead">
      <Select id="rules-lead" options={leadOptions(Number(minLead))} bind:value={minLead} />
    </FormField>
    <FormField label="¿Hasta qué fecha se puede reservar?" id="rules-days">
      <Select id="rules-days" options={aheadOptions(Number(maxDays))} bind:value={maxDays} />
    </FormField>
    <FormField
      label="¿Hasta cuándo puede el cliente cambiar o cancelar?"
      id="rules-cancel"
      hint="Después de ese plazo tiene que llamar a la barbería"
    >
      <Select id="rules-cancel" options={cancelOptions(Number(cancelLimit))} bind:value={cancelLimit} />
    </FormField>
  </fieldset>
  <div>
    <Button type="submit" disabled={settings.saving}>Guardar cambios</Button>
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
