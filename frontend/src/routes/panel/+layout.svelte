<!-- Marco del panel: exige sesión y muestra la barra con el menú según el rol. -->
<script lang="ts">
  import {goto} from '$app/navigation';
  import {resolve} from '$app/paths';
  import {page} from '$app/state';
  import {onMount} from 'svelte';
  import {session} from '#lib/core/session.svelte.ts';
  import {getShop} from '#lib/features/shop/api.ts';
  import PanelHeader from '#lib/ui/organisms/PanelHeader.svelte';
  import PanelLayout from '#lib/ui/templates/PanelLayout.svelte';
  import type {LayoutProps} from './$types';

  const {children}: LayoutProps = $props();

  const ownerLinks = [
    {href: '/panel/agenda', label: 'Agenda'},
    {href: '/panel/servicios', label: 'Servicios'},
    {href: '/panel/barberos', label: 'Barberos'},
    {href: '/panel/horarios', label: 'Horarios'},
    {href: '/panel/ajustes', label: 'Mi barbería'},
  ];
  const barberLinks = [{href: '/panel/agenda', label: 'Mi agenda'}];

  let shopName = $state('Barbería');
  let ready = $state(false);

  onMount(() => {
    session.load();
    if (!session.current) {
      goto(resolve('/ingresar'));
      return;
    }
    ready = true;
    getShop().then((result) => {
      if (result.ok) {
        shopName = result.data.name;
      }
    });
  });

  /** Menú que corresponde al rol de la sesión. */
  function links() {
    if (session.isOwner) {
      return ownerLinks;
    }
    return barberLinks;
  }

  /** Cierra la sesión y vuelve al inicio de sesión. */
  function logout(): void {
    session.close();
    goto(resolve('/ingresar'));
  }
</script>

{#if ready && session.current}
  <PanelLayout>
    {#snippet header()}
      <PanelHeader
        {shopName}
        links={links()}
        currentPath={page.url.pathname}
        userLabel={session.current?.email ?? ''}
        onLogout={logout}
      />
    {/snippet}
    {@render children()}
  </PanelLayout>
{/if}
