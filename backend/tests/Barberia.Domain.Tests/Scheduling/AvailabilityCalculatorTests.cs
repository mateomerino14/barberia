using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Domain.Tests.Scheduling;

/// <summary>Pruebas del cálculo de horas libres.</summary>
public sealed class AvailabilityCalculatorTests
{
    private const int FortyFive = 45;
    private const int HalfHourStep = 30;
    private static readonly DateOnly Saturday = new(2026, 11, 14);
    private static readonly DateOnly Friday = new(2026, 11, 13);
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly TenFifteen = new(10, 15);
    private static readonly TimeOnly TenTwenty = new(10, 20);
    private static readonly TimeOnly TenThirty = new(10, 30);
    private static readonly TimeOnly Eleven = new(11, 0);
    private static readonly TimeOnly ElevenThirty = new(11, 30);
    private static readonly TimeOnly Noon = new(12, 0);
    private static readonly TimeOnly FourFifteen = new(16, 15);
    private static readonly TimeOnly Five = new(17, 0);
    private static readonly TimeOnly Eight = new(20, 0);

    private readonly Shop _shop = Shop.Create("Barbería").Value;
    private readonly Barber _barber = Barber.Create("Lucho").Value;

    /// <summary>Prepara una barbería que abre los sábados de 9 a 20 y un barbero que trabaja de 10 a 17.</summary>
    public AvailabilityCalculatorTests()
    {
        _shop.SetOpeningHours([DailyHours.Create(DayOfWeek.Saturday, Nine, Eight).Value]);
        _barber.SetSchedule([DailyHours.Create(DayOfWeek.Saturday, Ten, Five).Value], _shop.OpeningHours.ToList());
    }

    /// <summary>Sin turnos, las horas van del inicio del barbero hasta la última en la que el servicio termina a tiempo.</summary>
    [Fact]
    public void FreeStarts_EmptyDay_CoversWholeShiftInSteps()
    {
        IReadOnlyList<DateTimeOffset> starts = Calculate([], At(Friday, Noon));

        Assert.Equal(At(Saturday, Ten), starts[0]);
        Assert.Equal(At(Saturday, FourFifteen), starts[^1]);
        Assert.All(starts.Zip(starts.Skip(1)), pair => Assert.Equal(TimeSpan.FromMinutes(BookingRules.DefaultSlotStepMinutes), pair.Second - pair.First));
    }

    /// <summary>Con turnos cada 30 minutos, las horas de inicio van de media en media hora.</summary>
    [Fact]
    public void FreeStarts_HalfHourStep_StartsEveryThirtyMinutes()
    {
        _shop.UpdateRules(BookingRules.Create(0, BookingRules.Default.MaxDaysAhead, 0, HalfHourStep).Value);

        IReadOnlyList<DateTimeOffset> starts = Calculate([], At(Friday, Noon));

        Assert.Equal([At(Saturday, Ten), At(Saturday, TenThirty), At(Saturday, Eleven)], starts.Take(3));
        Assert.All(starts.Zip(starts.Skip(1)), pair => Assert.Equal(TimeSpan.FromMinutes(HalfHourStep), pair.Second - pair.First));
    }

    /// <summary>Un turno ocupado saca las horas que chocan, pero no las que solo lo tocan en el borde.</summary>
    [Fact]
    public void FreeStarts_BusyHour_SkipsOverlapsOnly()
    {
        TimeRange busy = TimeRange.Create(At(Saturday, Eleven), At(Saturday, Noon)).Value;

        IReadOnlyList<DateTimeOffset> starts = Calculate([busy], At(Friday, Noon));

        Assert.Contains(At(Saturday, TenFifteen), starts);
        Assert.DoesNotContain(At(Saturday, TenThirty), starts);
        Assert.DoesNotContain(At(Saturday, ElevenThirty), starts);
        Assert.Contains(At(Saturday, Noon), starts);
    }

    /// <summary>El mismo día no se ofrecen horas antes de la anticipación mínima.</summary>
    [Fact]
    public void FreeStarts_SameDay_RespectsMinimumLead()
    {
        IReadOnlyList<DateTimeOffset> starts = Calculate([], At(Saturday, TenTwenty));

        Assert.Equal(At(Saturday, ElevenThirty), starts[0]);
    }

    /// <summary>Días cerrados, ausencias, barberos inactivos y fechas fuera de la ventana no tienen horas.</summary>
    [Fact]
    public void FreeStarts_DayNotBookable_IsEmpty()
    {
        DateTimeOffset now = At(Friday, Noon);
        DateOnly tooFar = Saturday.AddDays(BookingRules.Default.MaxDaysAhead + 1);

        Assert.Empty(AvailabilityCalculator.FreeStarts(_shop, _barber, Friday, FortyFive, [], now));
        Assert.Empty(AvailabilityCalculator.FreeStarts(_shop, _barber, tooFar, FortyFive, [], now));
        _shop.AddClosedDay(Saturday, "Feriado");
        Assert.Empty(Calculate([], now));
        _shop.RemoveClosedDay(Saturday);
        _barber.AddTimeOff(Saturday, Saturday, "Trámite");
        Assert.Empty(Calculate([], now));
    }

    /// <summary>Un barbero inactivo no recibe turnos.</summary>
    [Fact]
    public void FreeStarts_InactiveBarber_IsEmpty()
    {
        _barber.SetActive(false);

        Assert.Empty(Calculate([], At(Friday, Noon)));
    }

    /// <summary>Verificar una hora concreta usa las mismas reglas que la lista.</summary>
    [Fact]
    public void IsFree_MatchesList()
    {
        TimeRange busy = TimeRange.Create(At(Saturday, Eleven), At(Saturday, Noon)).Value;
        DateTimeOffset now = At(Friday, Noon);

        Assert.True(AvailabilityCalculator.IsFree(_shop, _barber, At(Saturday, Noon), FortyFive, [busy], now));
        Assert.False(AvailabilityCalculator.IsFree(_shop, _barber, At(Saturday, TenThirty), FortyFive, [busy], now));
        Assert.False(AvailabilityCalculator.IsFree(_shop, _barber, At(Saturday, TenTwenty), FortyFive, [busy], now));
    }

    /// <summary>Calcula las horas del sábado para el servicio de 45 minutos.</summary>
    private IReadOnlyList<DateTimeOffset> Calculate(IEnumerable<TimeRange> busy, DateTimeOffset now)
    {
        return AvailabilityCalculator.FreeStarts(_shop, _barber, Saturday, FortyFive, busy, now);
    }

    /// <summary>Instante en UTC de una fecha y hora de La Paz.</summary>
    private static DateTimeOffset At(DateOnly date, TimeOnly time)
    {
        return BusinessTime.At(date, time);
    }
}
