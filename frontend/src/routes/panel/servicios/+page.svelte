<!-- Servicios del panel: lista, alta y edición. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import {ServicesState} from '#lib/features/services/state.svelte.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PageTitle from '#lib/ui/molecules/PageTitle.svelte';
  import ServiceForm from '#lib/ui/organisms/ServiceForm.svelte';
  import ServiceTable from '#lib/ui/organisms/ServiceTable.svelte';

  const catalog = new ServicesState();

  onMount(() => {
    catalog.load();
  });
</script>

<svelte:head>
  <title>Servicios · Barbería</title>
</svelte:head>

<PageTitle kicker="Catálogo" title="Servicios">
  {#snippet actions()}
    {#if !catalog.creating && !catalog.editingId}
      <Button onclick={() => catalog.startCreate()}>Nuevo servicio</Button>
    {/if}
  {/snippet}
</PageTitle>

{#if catalog.error}
  <Notice>{catalog.error}</Notice>
{/if}

{#if catalog.creating || catalog.editing}
  <section class="sheet" aria-label="Formulario del servicio">
    <ServiceForm
      initial={catalog.editing}
      saving={catalog.saving}
      onSubmit={(values) => catalog.save(values)}
      onCancel={() => catalog.close()}
    />
  </section>
{/if}

{#if catalog.loading}
  <p class="loading">Cargando servicios…</p>
{:else}
  <ServiceTable services={catalog.services} onEdit={(id) => catalog.startEdit(id)} />
{/if}

<style>
  .sheet {
    padding: var(--space-5);
    border: var(--line-thin) solid var(--color-ink);
    background: var(--color-sheet);
  }

  .loading {
    margin: 0;
    color: var(--color-ink-soft);
  }
</style>
