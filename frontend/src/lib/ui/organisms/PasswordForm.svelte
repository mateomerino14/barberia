<!-- Cambio de la propia contraseña: la actual, la nueva y repetirla. -->
<script lang="ts">
  import Button from '#lib/ui/atoms/Button.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';

  interface Props {
    saving?: boolean;
    onSubmit: (current: string, next: string) => Promise<boolean>;
  }

  const {saving = false, onSubmit}: Props = $props();

  const passwordMinLength = 8;

  let current = $state('');
  let next = $state('');
  let repeat = $state('');
  let mismatch = $state(false);

  /** Revisa que las dos nuevas coincidan y envía el cambio. */
  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    mismatch = next !== repeat;
    if (mismatch) {
      return;
    }
    if (await onSubmit(current, next)) {
      current = '';
      next = '';
      repeat = '';
    }
  }

  /** Error del campo de repetir, si no coincide. */
  function repeatError(): string | null {
    if (mismatch) {
      return 'No coincide con la contraseña nueva.';
    }
    return null;
  }
</script>

<form class="password" onsubmit={submit}>
  <FormField label="Contraseña actual" id="password-current">
    <Input id="password-current" type="password" required autocomplete="current-password" bind:value={current} />
  </FormField>
  <FormField label="Contraseña nueva" id="password-new" hint="Al menos 8 caracteres">
    <Input
      id="password-new"
      type="password"
      required
      minlength={passwordMinLength}
      autocomplete="new-password"
      bind:value={next}
    />
  </FormField>
  <FormField label="Repite la contraseña nueva" id="password-repeat" error={repeatError()}>
    <Input
      id="password-repeat"
      type="password"
      required
      autocomplete="new-password"
      invalid={mismatch}
      bind:value={repeat}
    />
  </FormField>
  <div>
    <Button type="submit" disabled={saving}>Cambiar contraseña</Button>
  </div>
</form>

<style>
  .password {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }
</style>
