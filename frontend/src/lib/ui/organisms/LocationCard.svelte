<!-- Dónde queda la barbería: un plano dibujado, la dirección y botones para llegar o escribir. -->
<script lang="ts">
  import {whatsappHref} from '#lib/core/whatsapp.ts';
  import {directionsHref} from '#lib/core/maps.ts';
  import Button from '#lib/ui/atoms/Button.svelte';

  interface Props {
    name: string;
    address: string;
    phone: string;
  }

  const {name, address, phone}: Props = $props();
</script>

<div class="location">
  <div class="map" aria-hidden="true">
    <span class="street horizontal"></span>
    <span class="street vertical"></span>
    <span class="street diagonal"></span>
    <span class="pin"></span>
  </div>
  <div class="info">
    <p class="address">{address}</p>
    {#if phone}
      <p class="phone">{phone}</p>
    {/if}
    <div class="actions">
      <Button href={directionsHref(address)} target="_blank" rel="noopener">Cómo llegar</Button>
      {#if phone}
        <Button
          variant="secondary"
          href={whatsappHref(phone, `Hola, quiero consultar en ${name}.`)}
          target="_blank"
          rel="noopener"
        >
          WhatsApp
        </Button>
      {/if}
    </div>
  </div>
</div>

<style>
  .location {
    display: grid;
    grid-template-columns: 1fr;
    border: var(--line-bold) solid var(--color-ink);
    background: var(--color-sheet);
  }

  .map {
    position: relative;
    min-height: 12rem;
    overflow: hidden;
    background-color: var(--color-paper);
    background-image:
      linear-gradient(var(--color-rule) var(--line-thin), transparent var(--line-thin)),
      linear-gradient(90deg, var(--color-rule) var(--line-thin), transparent var(--line-thin));
    background-size: var(--space-6) var(--space-6);
    border-bottom: var(--line-bold) solid var(--color-ink);
  }

  .street {
    position: absolute;
    background: var(--color-sheet);
    border: var(--line-thin) solid var(--color-rule);
  }

  .horizontal {
    left: -10%;
    right: -10%;
    top: 55%;
    height: var(--space-4);
  }

  .vertical {
    top: -10%;
    bottom: -10%;
    left: 40%;
    width: var(--space-4);
  }

  .diagonal {
    left: -10%;
    right: -10%;
    top: 30%;
    height: var(--space-3);
    transform: rotate(-18deg);
  }

  .pin {
    position: absolute;
    left: calc(40% + var(--space-2));
    top: calc(55% + var(--space-2));
    width: var(--space-6);
    height: var(--space-6);
    background: var(--color-action);
    border: var(--line-bold) solid var(--color-ink);
    border-radius: 50% 50% 50% 0;
    transform: translate(-50%, -100%) rotate(-45deg);
  }

  .info {
    padding: var(--space-5);
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .address {
    margin: 0;
    font-size: var(--text-xl);
    font-weight: 800;
    font-stretch: var(--stretch-button);
  }

  .phone {
    margin: 0;
    font-family: var(--font-mono);
    color: var(--color-ink-soft);
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  @media (min-width: 48rem) {
    .location {
      grid-template-columns: 3fr 2fr;
    }

    .map {
      border-bottom: none;
      border-right: var(--line-bold) solid var(--color-ink);
    }
  }
</style>
