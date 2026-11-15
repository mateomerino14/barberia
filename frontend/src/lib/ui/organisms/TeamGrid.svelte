<!-- El equipo de barberos, cada uno con su foto o, si no tiene, sus iniciales en un retrato. -->
<script lang="ts">
  import type {PublicBarber} from '#lib/features/booking/types.ts';
  import {initials} from '#lib/features/landing/opening.ts';
  import {imageUrl} from '#lib/features/media/api.ts';

  interface Props {
    barbers: PublicBarber[];
  }

  const {barbers}: Props = $props();
</script>

<ul class="team">
  {#each barbers as barber (barber.id)}
    <li class="member">
      {#if barber.photoImageId}
        <img class="portrait photo" src={imageUrl(barber.photoImageId)} alt="" loading="lazy" />
      {:else}
        <span class="portrait" aria-hidden="true">{initials(barber.name)}</span>
      {/if}
      <span class="name">{barber.name}</span>
    </li>
  {/each}
</ul>

<style>
  .team {
    margin: 0;
    padding: 0;
    list-style: none;
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(9rem, 1fr));
    gap: var(--space-4);
  }

  .member {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .portrait {
    aspect-ratio: 4 / 5;
    display: grid;
    place-items: center;
    border: var(--line-firm) solid var(--color-ink);
    background: var(--color-sheet);
    font-size: var(--text-display);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    color: var(--color-action);
    box-shadow: var(--space-2) var(--space-2) 0 var(--color-rule);
  }

  .photo {
    width: 100%;
    object-fit: cover;
  }

  .name {
    font-weight: 800;
    font-stretch: var(--stretch-button);
    text-transform: uppercase;
  }
</style>
