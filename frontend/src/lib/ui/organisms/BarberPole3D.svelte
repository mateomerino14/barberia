<!-- Poste de barbería en 3D con los colores de la paleta; gira y se inclina con el teléfono. -->
<script lang="ts">
  import {onMount} from 'svelte';
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';

  interface Props {
    label?: string;
  }

  const {label = 'Poste de barbería girando'}: Props = $props();

  const pole = {
    radius: 0.55,
    glassRadius: 0.63,
    height: 3.6,
    capRadius: 0.78,
    capHeight: 0.32,
    ballRadius: 0.26,
    segments: 64,
    cameraFov: 30,
    cameraDistance: 9.5,
    near: 0.1,
    far: 100,
    lean: -0.18,
    stripeSpeed: 0.18,
    spinSpeed: 0.25,
    tiltLimit: 0.35,
    tiltEase: 0.06,
    degreesToTilt: 0.012,
    textureSize: 256,
    stripeWidth: 32,
    textureRepeat: 3,
    glassOpacity: 0.22,
    glassRoughness: 0.04,
    glassReflection: 0.35,
    stripeRoughness: 0.55,
    metalRoughness: 0.22,
    ambient: 0.2,
    keyLight: 1.1,
    rimLight: 0.8,
    keyX: 3,
    keyY: 4,
    keyZ: 6,
    rimX: -4,
    rimY: 1,
    rimZ: -3,
    capOffset: 0.16,
    ballOffset: 0.26,
    maxPixelRatio: 2,
    msPerSecond: 1000,
  };

  let canvas: HTMLCanvasElement | undefined = $state();
  let fallback = $state(false);

  /** Lee un color de los tokens de la página. */
  function token(name: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  }

  /** Dibuja las franjas diagonales del poste con los colores actuales. */
  function paintStripes(target: HTMLCanvasElement): void {
    const context = target.getContext('2d');
    if (!context) {
      return;
    }
    const size = pole.textureSize;
    const colors = [token('--color-action'), token('--color-pole-light'), token('--color-selected'), token('--color-pole-light')];
    context.fillStyle = colors[1];
    context.fillRect(0, 0, size, size);
    const bands = size / pole.stripeWidth;
    for (let index = -bands - colors.length; index < bands + colors.length; index++) {
      context.fillStyle = colors[((index % colors.length) + colors.length) % colors.length];
      context.beginPath();
      const start = index * pole.stripeWidth;
      context.moveTo(start, 0);
      context.lineTo(start + pole.stripeWidth, 0);
      context.lineTo(start + pole.stripeWidth + size, size);
      context.lineTo(start + size, size);
      context.closePath();
      context.fill();
    }
  }

  /** Indica si el navegador puede dibujar en 3D. */
  function supportsWebGl(): boolean {
    try {
      return Boolean(document.createElement('canvas').getContext('webgl2'));
    }
    catch {
      return false;
    }
  }

  onMount(() => {
    if (!canvas || !supportsWebGl()) {
      fallback = true;
      return;
    }
    let cleanup = () => {};
    const target = canvas;
    import('./barber-pole-scene.ts').then(({createPoleScene}) => {
      const stripes = document.createElement('canvas');
      stripes.width = pole.textureSize;
      stripes.height = pole.textureSize;
      paintStripes(stripes);
      const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;
      const scene = createPoleScene(target, stripes, pole, reduceMotion);
      const observer = new MutationObserver(() => {
        paintStripes(stripes);
        scene.refreshStripes();
      });
      observer.observe(document.documentElement, {attributes: true, attributeFilter: ['data-palette', 'data-theme']});
      cleanup = () => {
        observer.disconnect();
        scene.dispose();
      };
    }).catch(() => {
      fallback = true;
    });
    return () => cleanup();
  });
</script>

<div class="stage" role="img" aria-label={label}>
  {#if fallback}
    <div class="flat">
      <PoleStripe size="thick" />
    </div>
  {:else}
    <canvas class="canvas" bind:this={canvas}></canvas>
  {/if}
</div>

<style>
  .stage {
    width: 100%;
    height: 100%;
    min-height: 16rem;
    display: grid;
    place-items: center;
  }

  .canvas {
    width: 100%;
    height: 100%;
    display: block;
  }

  .flat {
    width: 3rem;
    height: 14rem;
    display: flex;
    flex-direction: column;
    transform: rotate(-10deg);
  }

  .flat :global(div) {
    flex: 1;
  }
</style>
