<!-- El equipo de barberos, cada uno con sus iniciales en un retrato. -->
<script lang="ts">
  import type {PublicBarber} from '#lib/features/booking/types.ts';
  import {initials} from '#lib/features/landing/opening.ts';

  interface Props {
    barbers: PublicBarber[];
  }

  const {barbers}: Props = $props();
</script>

<ul class="team">
  {#each barbers as barber (barber.id)}
    <li class="member">
      <span class="portrait" aria-hidden="true">{initials(barber.name)}</span>
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

  .name {
    font-weight: 800;
    font-stretch: var(--stretch-button);
    text-transform: uppercase;
  }
</style>
