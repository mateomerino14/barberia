<!-- Mi barbería: los datos que ve el cliente y las reglas para reservar. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import {theme, type Palette} from '#lib/core/theme.svelte.ts';
  import {paletteOptions} from '#lib/features/shop/palette-options.ts';
  import {aheadOptions, cancelOptions, leadOptions} from '#lib/features/shop/rule-options.ts';
  import {ShopState} from '#lib/features/shop/state.svelte.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import Select from '#lib/ui/atoms/Select.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';
  import {imageUrl} from '#lib/features/media/api.ts';
  import ImagePicker from '#lib/ui/molecules/ImagePicker.svelte';
  import GalleryEditor from '#lib/ui/organisms/GalleryEditor.svelte';
  import PalettePicker from '#lib/ui/organisms/PalettePicker.svelte';

  const settings = new ShopState();
  const maxPhotos = 6;
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
  let palette = $state<Palette>('classic');

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
        palette = settings.shop.palette;
      }
    });
  });

  /** Dirección del logo actual, o null. */
  function logoSrc(): string | null {
    const id = settings.shop?.logoImageId;
    if (!id) {
      return null;
    }
    return imageUrl(id);
  }

  /** Fotos del local con su dirección. */
  function galleryPhotos() {
    return (settings.shop?.galleryImageIds ?? []).map((id) => ({id, src: imageUrl(id)}));
  }

  /** Guarda los datos y las reglas. */
  function submit(event: SubmitEvent): void {
    event.preventDefault();
    settings.saveProfile({
      name,
      address,
      phone,
      welcomeMessage,
      rules: {minLeadMinutes: Number(minLead), maxDaysAhead: Number(maxDays), cancelLimitMinutes: Number(cancelLimit)},
      palette,
    }).then(() => theme.setPalette(palette));
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
  <fieldset class="group">
    <legend>Colores</legend>
    <p class="help">
      Se aplican a la presentación, la reserva, el turno del cliente y este panel.
      El modo oscuro lo elige cada persona con el botón de sol o luna.
    </p>
    <PalettePicker
      options={paletteOptions}
      selected={palette}
      shopName={name || 'Barbería'}
      onSelect={(id) => (palette = id as Palette)}
    />
  </fieldset>
  <div>
    <Button type="submit" disabled={settings.saving}>Guardar cambios</Button>
  </div>
</form>

<section class="group images" aria-labelledby="images-title">
  <h2 class="group-title" id="images-title">Logo y fotos</h2>
  <p class="help">Se guardan al elegirlas. Las achicamos en tu teléfono para que la página cargue rápido.</p>
  <h3 class="label">Logo</h3>
  <ImagePicker
    src={logoSrc()}
    alt="Logo de {name || 'la barbería'}"
    emptyText="Sin logo"
    pickLabel="Subir logo"
    busy={settings.saving}
    onPick={(file) => settings.uploadLogo(file)}
    onRemove={() => settings.removeLogo()}
  />
  <h3 class="label">Fotos del local</h3>
  <GalleryEditor
    photos={galleryPhotos()}
    max={maxPhotos}
    busy={settings.saving}
    onAdd={(file) => settings.addPhoto(file)}
    onRemove={(id) => settings.removePhoto(id)}
  />
</section>

<style>
  .images {
    max-width: 60rem;
    margin-top: var(--space-5);
  }

  .group-title {
    margin: 0;
    font-size: var(--text-md);
    font-weight: 800;
    text-transform: uppercase;
  }

  .label {
    margin: var(--space-2) 0 0;
    font-size: var(--text-sm);
    font-weight: 700;
  }

  .form {
    display: flex;
    flex-direction: column;
    gap: var(--space-5);
    max-width: 60rem;
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

  .help {
    margin: 0;
    font-size: var(--text-sm);
    color: var(--color-ink-soft);
  }

  legend {
    padding: 0 var(--space-2);
    font-weight: 800;
    font-stretch: var(--stretch-button);
    text-transform: uppercase;
  }
</style>
