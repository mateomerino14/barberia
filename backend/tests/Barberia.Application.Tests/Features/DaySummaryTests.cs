using Barberia.Application.Features.Agenda;
using Barberia.Application.Features.Bookings;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas del resumen de días: cerrados, llenos y con lugar.</summary>
public sealed class DaySummaryTests
{
    private const string AllDay = "Capacitación";
    private static readonly DateOnly Monday = FakeClock.Today.AddDays(-1);
    private static readonly DateOnly Wednesday = FakeClock.Today.AddDays(1);
    private static readonly DateOnly NextMonday = FakeClock.Today.AddDays(6);
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Seven = new(19, 0);
    private static readonly StaffActor Owner = new(StaffRole.Owner, null);

    /// <summary>Empieza hoy, llega hasta el límite de días y marca cerrados los lunes.</summary>
    [Fact]
    public async Task Summary_CoversWindowAndClosedDays()
    {
        DemoWorld world = await DemoWorld.CreateAsync();

        IReadOnlyList<DaySummary> days = await SummaryAsync(world, null, null);

        Assert.Equal(FakeClock.Today, days[0].Date);
        Assert.Equal(world.Store.Shop!.Rules.MaxDaysAhead + 1, days.Count);
        DaySummary closed = Assert.Single(days, day => day.Date == NextMonday);
        Assert.False(closed.Open);
        Assert.Equal(0, closed.FreeStarts);
        Assert.DoesNotContain(days, day => day.Date == Monday);
    }

    /// <summary>Si el barbero elegido tiene el día bloqueado entero, ese día está lleno con él pero no con cualquiera.</summary>
    [Fact]
    public async Task Summary_FullDayForOneBarber()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        await new BlockHandler(world.Store, world.Store)
            .AddAsync(new AddBlockCommand(world.Lucho.Id, Wednesday, Nine, Seven, AllDay), Owner, TestContext.Current.CancellationToken);

        DaySummary withLucho = Assert.Single(await SummaryAsync(world, world.Haircut.Id, world.Lucho.Id), day => day.Date == Wednesday);
        DaySummary withAnyone = Assert.Single(await SummaryAsync(world, world.Haircut.Id, null), day => day.Date == Wednesday);

        Assert.True(withLucho.Open);
        Assert.Equal(0, withLucho.FreeStarts);
        Assert.True(withAnyone.FreeStarts > 0);
    }

    /// <summary>Un servicio que no existe o está oculto da error.</summary>
    [Fact]
    public async Task Summary_UnknownService_Fails()
    {
        DemoWorld world = await DemoWorld.CreateAsync();

        Result<IReadOnlyList<DaySummary>> result = await new GetDaySummaryHandler(world.Store, world.Store, world.Store, world.Schedule, world.Clock)
            .HandleAsync(new GetDaySummaryQuery(Guid.NewGuid(), null), TestContext.Current.CancellationToken);

        Assert.Equal(SchedulingErrors.ServiceUnavailable, result.Error);
    }

    /// <summary>Pide el resumen de días.</summary>
    private static async Task<IReadOnlyList<DaySummary>> SummaryAsync(DemoWorld world, Guid? serviceId, Guid? barberId)
    {
        GetDaySummaryHandler handler = new(world.Store, world.Store, world.Store, world.Schedule, world.Clock);
        return (await handler.HandleAsync(new GetDaySummaryQuery(serviceId, barberId), TestContext.Current.CancellationToken)).Value;
    }
}
