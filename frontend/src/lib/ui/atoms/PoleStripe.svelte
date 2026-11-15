<!-- Franja del poste de barbería, solo decorativa. -->
<script lang="ts">
  type Size = 'thin' | 'thick';

  interface Props {
    size?: Size;
  }

  const {size = 'thin'}: Props = $props();
</script>

<div class="pole {size}" aria-hidden="true"></div>

<style>
  .pole {
    background: var(--pole-stripes);
  }

  .thin {
    height: var(--space-2);
  }

  .thick {
    height: var(--space-4);
    border: var(--line-thin) solid var(--color-ink);
  }
</style>
