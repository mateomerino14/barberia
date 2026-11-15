<!-- Marco común de todas las páginas: estilos base, modo de color y paleta de la barbería. -->
<script lang="ts">
  import '@fontsource-variable/archivo/wdth.css';
  import '@fontsource/ibm-plex-mono/400.css';
  import '@fontsource/ibm-plex-mono/600.css';
  import '#lib/styles/global.css';
  import {onMount} from 'svelte';
  import {theme} from '#lib/core/theme.svelte.ts';
  import {getShop} from '#lib/features/shop/api.ts';
  import type {LayoutProps} from './$types';

  const {children}: LayoutProps = $props();

  onMount(() => {
    theme.init();
    getShop().then((result) => {
      if (result.ok) {
        theme.setPalette(result.data.palette);
      }
    });
  });
</script>

{@render children()}
