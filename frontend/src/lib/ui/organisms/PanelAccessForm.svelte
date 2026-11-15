<!-- Acceso al panel de un barbero: darle cuenta si no tiene, o ponerle una contraseña nueva si la olvidó. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';

  interface Props {
    email: string | null;
    saving?: boolean;
    onSubmit: (email: string | null, password: string) => Promise<boolean>;
  }

  const {email, saving = false, onSubmit}: Props = $props();

  const passwordMinLength = 8;

  let newEmail = $state('');
  let password = $state('');

  /** Envía el acceso y limpia la contraseña si salió bien. */
  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    let target: string | null = null;
    if (!email) {
      target = newEmail;
    }
    if (await onSubmit(target, password)) {
      password = '';
    }
  }
</script>

<form class="access" onsubmit={submit}>
  {#if email}
    <p class="state">Entra con <strong class="mono">{email}</strong>.</p>
    <FormField label="Contraseña nueva" id="access-password" hint="Si la olvidó. Al menos 8 caracteres.">
      <Input
        id="access-password"
        type="password"
        required
        minlength={passwordMinLength}
        autocomplete="new-password"
        bind:value={password}
      />
    </FormField>
    <div>
      <Button variant="secondary" type="submit" disabled={saving}>Cambiar contraseña</Button>
    </div>
  {:else}
    <p class="state">Todavía no puede entrar al panel para ver su agenda.</p>
    <div class="pair">
      <FormField label="Correo" id="access-email">
        <Input id="access-email" type="email" required autocomplete="off" bind:value={newEmail} />
      </FormField>
      <FormField label="Contraseña" id="access-new" hint="Al menos 8 caracteres">
        <Input
          id="access-new"
          type="password"
          required
          minlength={passwordMinLength}
          autocomplete="new-password"
          bind:value={password}
        />
      </FormField>
    </div>
    <div>
      <Button variant="secondary" type="submit" disabled={saving}>Darle acceso</Button>
    </div>
  {/if}
</form>

<style>
  .access {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .state {
    margin: 0;
    color: var(--color-ink-soft);
  }

  .mono {
    font-family: var(--font-mono);
    color: var(--color-ink);
  }

  .pair {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(12rem, 1fr));
    gap: var(--space-3);
  }
</style>
