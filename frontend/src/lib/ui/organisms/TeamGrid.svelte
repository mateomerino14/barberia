<!-- El equipo de barberos, cada uno con su foto o, si no tiene, sus iniciales en un retrato. -->
<script lang="ts">
  import {initials} from '#lib/core/text.ts';

  interface Member {
    id: string;
    name: string;
    photoSrc: string | null;
  }

  interface Props {
    members: Member[];
  }

  const {members}: Props = $props();
</script>

<ul class="team">
  {#each members as member (member.id)}
    <li class="member">
      {#if member.photoSrc}
        <img class="portrait photo" src={member.photoSrc} alt="" loading="lazy" />
      {:else}
        <span class="portrait" aria-hidden="true">{initials(member.name)}</span>
      {/if}
      <span class="name">{member.name}</span>
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
