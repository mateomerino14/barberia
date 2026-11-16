<!-- Pedir por correo el enlace para crear una contraseña nueva; al enviarlo dice lo mismo exista o no la cuenta. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';

  interface Props {
    sent?: boolean;
    busy?: boolean;
    minutes: number;
    onSubmit: (email: string) => void;
    onBack: () => void;
  }

  const {sent = false, busy = false, minutes, onSubmit, onBack}: Props = $props();

  let email = $state('');

  /** Pide el enlace para el correo escrito. */
  function submit(event: SubmitEvent): void {
    event.preventDefault();
    onSubmit(email);
  }
</script>

<form class="forgot" onsubmit={submit}>
  {#if sent}
    <Notice tone="success">
      Si ese correo tiene cuenta, te llegó un enlace para crear una contraseña nueva. Vence en {minutes} minutos.
    </Notice>
  {:else}
    <p class="help">Escribe el correo con el que entras y te mandamos un enlace para crear una contraseña nueva.</p>
    <FormField label="Correo" id="forgot-email">
      <Input id="forgot-email" type="email" autocomplete="username" required bind:value={email} />
    </FormField>
    <Button type="submit" disabled={busy}>Mandarme el enlace</Button>
  {/if}
  <Button variant="secondary" onclick={onBack}>Volver a ingresar</Button>
</form>

<style>
  .forgot {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .help {
    margin: 0;
    color: var(--color-ink-soft);
  }
</style>
