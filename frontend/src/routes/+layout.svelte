<!-- Marco común de todas las páginas: carga los estilos base. -->
<script lang="ts">
  import '@fontsource-variable/archivo/wdth.css';
  import '@fontsource/ibm-plex-mono/400.css';
  import '@fontsource/ibm-plex-mono/600.css';
  import '#lib/styles/global.css';
  import type {LayoutProps} from './$types';

  const {children}: LayoutProps = $props();
</script>

{@render children()}
