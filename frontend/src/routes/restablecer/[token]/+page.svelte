<!-- Crear una contraseña nueva con el enlace que llegó por correo. -->
<script lang="ts">
  import {resolve} from '$app/paths';
  import {page} from '$app/state';
  import {confirmPasswordReset} from '#lib/features/auth/api.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';
  import PasswordForm from '#lib/ui/organisms/PasswordForm.svelte';

  let saving = $state(false);
  let done = $state(false);
  let error = $state<string | null>(null);

  /** Guarda la contraseña nueva con la clave del enlace. */
  async function save(_current: string, next: string): Promise<boolean> {
    saving = true;
    const result = await confirmPasswordReset(page.params.token ?? '', next);
    saving = false;
    if (!result.ok) {
      error = result.error.message;
      return false;
    }
    error = null;
    done = true;
    return true;
  }
</script>

<svelte:head>
  <title>Contraseña nueva · Barbería</title>
</svelte:head>

<PoleStripe />
<main class="reset">
  <span class="kicker">Panel de la barbería</span>
  <h1 class="title">Contraseña nueva</h1>
  {#if done}
    <Notice tone="success">
      Listo: ya puedes ingresar con tu contraseña nueva. Las sesiones abiertas se cerraron.
    </Notice>
    <Button href={resolve('/ingresar')}>Ingresar</Button>
  {:else}
    {#if error}
      <Notice>{error}</Notice>
    {/if}
    <PasswordForm askCurrent={false} {saving} onSubmit={save} />
    <Button variant="secondary" href={resolve('/ingresar')}>Volver a ingresar</Button>
  {/if}
</main>

<style>
  .reset {
    max-width: 24rem;
    margin: 0 auto;
    padding: var(--space-6) var(--space-5);
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .kicker {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--color-ink-soft);
  }

  .title {
    margin: 0;
    font-size: var(--text-display);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 0.95;
    text-transform: uppercase;
  }
</style>
