<!-- Entrada del panel: lleva a la agenda. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import {goto} from '$app/navigation';
  import {resolve} from '$app/paths';

  onMount(() => {
    goto(resolve('/panel/agenda'), {replaceState: true});
  });
</script>
