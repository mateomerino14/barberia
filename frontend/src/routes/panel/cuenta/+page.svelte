<!-- Mi cuenta: con qué correo entro y cambiar mi contraseña. -->
<script lang="ts">
  import {session} from '#lib/core/session.svelte.ts';
  import {changePassword} from '#lib/features/auth/api.ts';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';
  import PasswordForm from '#lib/ui/organisms/PasswordForm.svelte';

  let saving = $state(false);
  let error = $state<string | null>(null);
  let notice = $state<string | null>(null);

  /** Cambia la contraseña y guarda la sesión nueva que devuelve la API. */
  async function save(current: string, next: string): Promise<boolean> {
    saving = true;
    const result = await changePassword(current, next, session.token);
    saving = false;
    if (!result.ok) {
      error = result.error.message;
      notice = null;
      return false;
    }
    session.open(result.data);
    error = null;
    notice = 'Contraseña cambiada. Las sesiones abiertas en otros equipos se cerraron.';
    return true;
  }
</script>

<svelte:head>
  <title>Mi cuenta · Barbería</title>
</svelte:head>

<PageTitle kicker="Acceso" title="Mi cuenta" />

<section class="sheet">
  <p class="who">Entras con <strong class="mono">{session.current?.email}</strong></p>
  {#if error}
    <Notice>{error}</Notice>
  {/if}
  {#if notice}
    <Notice tone="success">{notice}</Notice>
  {/if}
  <PasswordForm {saving} onSubmit={save} />
</section>

<style>
  .sheet {
    max-width: 32rem;
    padding: var(--space-5);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .who {
    margin: 0;
    color: var(--color-ink-soft);
  }

  .mono {
    font-family: var(--font-mono);
    color: var(--color-ink);
  }
</style>
