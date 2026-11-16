<!-- Página pública de reservas: servicio, barbero, día y hora, y los datos del cliente. -->
<script lang="ts">
  import {goto} from '$app/navigation';
  import {resolve} from '$app/paths';
  import {onMount} from 'svelte';
  import {theme} from '#lib/core/theme.svelte.ts';
  import {formatPrice} from '#lib/core/money.ts';
  import {clockTime, formatDuration, formatLongDate, fromApiTime, todayInBolivia, weekdayOf} from '#lib/core/time.ts';
  import {BookingFlow} from '#lib/features/booking/state.svelte.ts';
  import Button from '#lib/ui/atoms/Button.svelte';
  import Checkbox from '#lib/ui/atoms/Checkbox.svelte';
  import Input from '#lib/ui/atoms/Input.svelte';
  import Notice from '#lib/ui/atoms/Notice.svelte';
  import PoleStripe from '#lib/ui/atoms/PoleStripe.svelte';
  import FormField from '#lib/ui/molecules/FormField.svelte';
  import StepTitle from '#lib/ui/molecules/StepTitle.svelte';
  import ThemeToggle from '#lib/ui/molecules/ThemeToggle.svelte';
  import BarberPicker from '#lib/ui/organisms/BarberPicker.svelte';
  import DayStrip from '#lib/ui/organisms/DayStrip.svelte';
  import ServicePicker from '#lib/ui/organisms/ServicePicker.svelte';
  import SlotGrid from '#lib/ui/organisms/SlotGrid.svelte';

  const flow = new BookingFlow();
  const nameMaxLength = 80;
  const phoneMaxLength = 20;

  onMount(() => {
    flow.load();
  });

  /** Texto del horario de hoy para la cabecera. */
  function todayHours(): string {
    const today = todayInBolivia();
    const hours = flow.shop?.openingHours.find((item) => item.day === weekdayOf(today));
    const closed = flow.shop?.closedDays.some((day) => day.date === today);
    if (!hours || closed) {
      return 'Hoy no abrimos';
    }
    return `Hoy atiende de ${fromApiTime(hours.opens)} a ${fromApiTime(hours.closes)}`;
  }

  /** Texto cuando no hay horas libres. */
  function emptySlotsText(): string {
    if (!flow.serviceId) {
      return 'Elige primero un servicio.';
    }
    return 'No quedan horas libres ese día. Prueba con otro día o con cualquier barbero.';
  }

  /** Envía la reserva y lleva al cliente a su turno. */
  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    const confirmation = await flow.submit();
    if (confirmation) {
      await goto(resolve('/turno/[token]', {token: confirmation.token}));
    }
  }
</script>

<svelte:head>
  <title>{flow.shop?.name ?? 'Barbería'} · Reserva tu turno</title>
</svelte:head>

<header class="header">
  <nav class="bar">
    <a class="back" href={resolve('/')}>← Inicio</a>
    <ThemeToggle dark={theme.isDark} onToggle={() => theme.toggle()} />
  </nav>
  {#if flow.shop?.address}
    <span class="kicker">{flow.shop.address}</span>
  {/if}
  <h1 class="title">{flow.shop?.name ?? 'Barbería'}</h1>
  {#if flow.shop}
    <span class="hours">{todayHours()}</span>
  {/if}
</header>
<PoleStripe />

<form class="page" onsubmit={submit}>
  {#if flow.shop?.welcomeMessage}
    <p class="welcome">{flow.shop.welcomeMessage}</p>
  {/if}

  {#if flow.loading}
    <p class="muted">Cargando…</p>
  {:else}
    <section class="step">
      <StepTitle step="01" title="¿Qué te haces?" />
      <ServicePicker services={flow.services} selectedId={flow.serviceId} onSelect={(id) => flow.chooseService(id)} />
    </section>

    <section class="step">
      <StepTitle step="02" title="¿Con quién?" />
      <BarberPicker
        barbers={flow.availableBarbers}
        selectedId={flow.barberId}
        onSelect={(id) => flow.chooseBarber(id)}
      />
    </section>

    <section class="step">
      <StepTitle step="03" title="¿Cuándo?" />
      <DayStrip
        days={flow.days}
        selected={flow.date}
        showCounts={Boolean(flow.serviceId)}
        onSelect={(date) => flow.chooseDate(date)}
      />
      <p class="muted date">{formatLongDate(flow.date)}</p>
      <SlotGrid
        slots={flow.slots}
        selected={flow.start}
        loading={flow.loadingSlots}
        emptyText={emptySlotsText()}
        onSelect={(start) => flow.chooseStart(start)}
      />
    </section>

    <section class="step">
      <StepTitle step="04" title="Tus datos" />
      <FormField label="Nombre" id="customer-name">
        <Input
          id="customer-name"
          autocomplete="given-name"
          required
          maxlength={nameMaxLength}
          bind:value={flow.customer.name}
        />
      </FormField>
      <FormField label="Celular" id="customer-phone" hint="Para avisarte si algo cambia">
        <Input
          id="customer-phone"
          type="tel"
          inputmode="tel"
          autocomplete="tel"
          mono
          required
          maxlength={phoneMaxLength}
          bind:value={flow.customer.phone}
        />
      </FormField>
      <FormField label="Correo (opcional)" id="customer-email" hint="Te mandamos ahí la confirmación">
        <Input
          id="customer-email"
          type="email"
          autocomplete="email"
          placeholder="tucorreo@ejemplo.com"
          bind:value={flow.customer.email}
        />
      </FormField>
      <Checkbox bind:checked={flow.remember}>Recordar mis datos en este teléfono</Checkbox>
    </section>
  {/if}

  <footer class="summary">
    {#if flow.error}
      <Notice>{flow.error}</Notice>
    {/if}
    {#if flow.service}
      <div class="line">
        <span><strong>{flow.service.name}</strong> con {flow.barberName}</span>
        {#if flow.start}
          <span class="mono">{clockTime(flow.start)} · {formatPrice(flow.service.priceCents)}</span>
        {/if}
      </div>
    {/if}
    <Button type="submit" disabled={!flow.ready || flow.sending}>Reservar turno</Button>
    {#if flow.shop}
      <span class="muted small">
        Pagas en la barbería. Puedes cambiar o cancelar hasta
        {formatDuration(flow.shop.rules.cancelLimitMinutes)} antes.
      </span>
    {/if}
  </footer>
</form>

<style>
  .header {
    padding: var(--space-5) var(--space-5) var(--space-4);
    background: var(--color-bar);
    color: var(--color-on-bar);
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-3);
  }

  .back {
    min-height: var(--touch-min);
    display: inline-flex;
    align-items: center;
    padding: 0 var(--space-3);
    border: var(--line-firm) solid var(--color-on-bar-soft);
    color: var(--color-on-bar);
    font-family: var(--font-mono);
    font-size: var(--text-sm);
    text-decoration: none;
    text-transform: uppercase;
    letter-spacing: 0.06em;
  }

  .back:hover {
    border-color: var(--color-on-bar);
  }

  .kicker {
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: var(--color-on-bar-soft);
  }

  .title {
    margin: 0;
    font-size: var(--text-display);
    font-weight: 900;
    font-stretch: var(--stretch-display);
    line-height: 0.95;
    text-transform: uppercase;
  }

  .hours {
    font-size: var(--text-sm);
    color: var(--color-on-bar-soft);
  }

  .page {
    max-width: 36rem;
    margin: 0 auto;
    padding: var(--space-5) var(--space-5) 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-6);
  }

  .welcome {
    margin: 0;
    font-size: var(--text-lg);
  }

  .step {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .muted {
    margin: 0;
    color: var(--color-ink-soft);
  }

  .date::first-letter {
    text-transform: uppercase;
  }

  .small {
    font-size: var(--text-sm);
    text-align: center;
  }

  .summary {
    position: sticky;
    bottom: 0;
    margin: 0 calc(var(--space-5) * -1);
    padding: var(--space-4) var(--space-5) var(--space-5);
    border-top: var(--line-bold) solid var(--color-ink);
    background: var(--color-sheet);
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .line {
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;
    gap: var(--space-2);
  }

  .mono {
    font-family: var(--font-mono);
  }
</style>
