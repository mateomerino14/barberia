<!-- Barra superior del panel: nombre de la barbería, menú según el rol y salida. -->
<script lang="ts">
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';

  interface NavLink {
    href: string;
    label: string;
  }

  interface Props {
    shopName: string;
    links: NavLink[];
    currentPath: string;
    userLabel: string;
    onLogout: () => void;
  }

  const {shopName, links, currentPath, userLabel, onLogout}: Props = $props();

  /** Indica si el enlace corresponde a la página abierta. */
  function isCurrent(href: string): boolean {
    return currentPath.startsWith(href);
  }
</script>

<header class="bar">
  <strong class="brand">{shopName}</strong>
  <nav aria-label="Panel" class="nav">
    {#each links as link (link.href)}
      {#if isCurrent(link.href)}
        <a class="link current" href={link.href} aria-current="page">{link.label}</a>
      {:else}
        <a class="link" href={link.href}>{link.label}</a>
      {/if}
    {/each}
  </nav>
  <div class="user">
    <span>{userLabel}</span>
    <button class="logout" type="button" onclick={onLogout}>Salir</button>
  </div>
</header>
<PoleStripe />

<style>
  .bar {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-2) var(--space-6);
    padding: 0 var(--space-5);
    background: var(--color-ink);
    color: var(--color-paper);
  }

  .brand {
    font-size: var(--text-xl);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    text-transform: uppercase;
  }

  .nav {
    display: flex;
    flex-wrap: wrap;
    flex: 1;
  }

  .link {
    padding: var(--space-4) var(--space-3);
    border-bottom: 3px solid transparent;
    color: var(--color-rule);
    text-decoration: none;
  }

  .link:hover {
    color: var(--color-sheet);
  }

  .current {
    border-bottom-color: var(--color-action);
    color: var(--color-sheet);
    font-weight: 700;
  }

  .user {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    font-size: var(--text-sm);
    color: var(--color-rule);
  }

  @media (max-width: 48rem) {
    .bar {
      padding: var(--space-2) var(--space-4) 0;
    }

    .user {
      margin-left: auto;
    }

    .user span {
      display: none;
    }

    .nav {
      order: 3;
      flex: 1 1 100%;
      flex-wrap: nowrap;
      overflow-x: auto;
    }

    .link {
      padding: var(--space-3) var(--space-3);
      white-space: nowrap;
    }
  }

  .logout {
    min-height: var(--touch-min);
    padding: 0 var(--space-3);
    border: var(--line-thin) solid var(--color-rule);
    background: transparent;
    color: var(--color-sheet);
    font-family: var(--font-sans);
    cursor: pointer;
  }
</style>
