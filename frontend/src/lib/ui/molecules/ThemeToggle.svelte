<!-- Botón de sol o luna para pasar entre modo claro y oscuro. -->
<script lang="ts">
  interface Props {
    dark: boolean;
    onToggle: () => void;
  }

  const {dark, onToggle}: Props = $props();

  /** Texto para lectores de pantalla según el modo actual. */
  function label(): string {
    if (dark) {
      return 'Cambiar a modo claro';
    }
    return 'Cambiar a modo oscuro';
  }
</script>

<button class="toggle" type="button" aria-label={label()} title={label()} onclick={onToggle}>
  {#if dark}
    <svg
      aria-hidden="true"
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
    >
      <circle cx="12" cy="12" r="4"></circle>
      <path
        d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"
      >
        </path>
    </svg>
  {:else}
    <svg
      aria-hidden="true"
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
    >
      <path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z"></path>
    </svg>
  {/if}
</button>

<style>
  .toggle {
    width: var(--touch-min);
    height: var(--touch-min);
    display: grid;
    place-items: center;
    border: var(--line-thin) solid var(--color-on-bar-soft);
    background: transparent;
    color: var(--color-on-bar);
    cursor: pointer;
  }
</style>
