<!-- Inicio de sesión del dueño y los barberos, con la opción de recuperar la contraseña por correo. -->
<script lang="ts">
  import {goto} from '$app/navigation';
  import {resolve} from '$app/paths';
  import {onMount} from 'svelte';
  import {session} from '#lib/core/session.svelte.ts';
  import {login, passwordResetAvailable, requestPasswordReset} from '#lib/features/auth/api.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';
  import ForgotPasswordForm from '#lib/ui/organisms/ForgotPasswordForm.svelte';

  // Lo que dura el enlace del correo, igual que en la API.
  const resetLinkMinutes = 30;

  let email = $state('');
  let canReset = $state(false);
  let forgotten = $state(false);
  let resetSent = $state(false);
  let password = $state('');
  let error = $state<string | null>(null);
  let sending = $state(false);

  onMount(async () => {
    canReset = await passwordResetAvailable();
  });

  /** Pide el enlace para crear una contraseña nueva. */
  async function requestReset(address: string): Promise<void> {
    sending = true;
    const result = await requestPasswordReset(address);
    sending = false;
    if (!result.ok) {
      error = result.error.message;
      return;
    }
    error = null;
    resetSent = true;
  }

  /** Vuelve al formulario para ingresar. */
  function backToLogin(): void {
    forgotten = false;
    resetSent = false;
  }

  /** Abre la sesión y entra al panel. */
  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    sending = true;
    const result = await login(email, password);
    sending = false;
    if (!result.ok) {
      error = result.error.message;
      return;
    }
    session.open(result.data);
    await goto(resolve('/panel'));
  }
</script>

<svelte:head>
  <title>Ingresar · Barbería</title>
</svelte:head>

<PoleStripe />
<main class="login">
  <span class="kicker">Panel de la barbería</span>
  <h1 class="title">Ingresar</h1>
  {#if forgotten}
    {#if error}
      <Notice>{error}</Notice>
    {/if}
    <ForgotPasswordForm
      sent={resetSent}
      busy={sending}
      minutes={resetLinkMinutes}
      onSubmit={requestReset}
      onBack={backToLogin}
    />
  {:else}
    <form class="form" onsubmit={submit}>
      {#if error}
        <Notice>{error}</Notice>
      {/if}
      <FormField label="Correo" id="login-email">
        <Input id="login-email" type="email" autocomplete="username" required bind:value={email} />
      </FormField>
      <FormField label="Contraseña" id="login-password">
        <Input id="login-password" type="password" autocomplete="current-password" required bind:value={password} />
      </FormField>
      <Button type="submit" disabled={sending}>Entrar</Button>
      {#if canReset}
        <Button variant="secondary" onclick={() => (forgotten = true)}>¿Olvidaste tu contraseña?</Button>
      {/if}
    </form>
  {/if}
</main>

<style>
  .login {
    max-width: 24rem;
    margin: 0 auto;
    padding: var(--space-6) var(--space-5);
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .kicker {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--color-ink-soft);
  }

  .title {
    margin: 0 0 var(--space-4);
    font-size: var(--text-display);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 0.95;
    text-transform: uppercase;
  }

  .form {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }
</style>
