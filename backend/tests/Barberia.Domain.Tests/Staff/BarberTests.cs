using Barberia.Domain.Common;
using Barberia.Domain.Staff;

namespace Barberia.Domain.Tests.Staff;

/// <summary>Pruebas del barbero, su horario y sus ausencias.</summary>
public sealed class BarberTests
{
    private const string BarberName = "Lucho";
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly Five = new(17, 0);
    private static readonly TimeOnly Eight = new(20, 0);
    private static readonly TimeOnly Nine30Night = new(21, 30);
    private static readonly DateOnly Saturday = new(2026, 11, 14);
    private static readonly DateOnly Sunday = new(2026, 11, 15);
    private static readonly DailyHours[] ShopHours =
    [
        DailyHours.Create(DayOfWeek.Saturday, Nine, Eight).Value,
    ];

    /// <summary>Un horario dentro del de la barbería se acepta y define el día de trabajo.</summary>
    [Fact]
    public void SetSchedule_InsideShopHours_IsUsedForThatDay()
    {
        Barber barber = Barber.Create(BarberName).Value;
        DailyHours shift = DailyHours.Create(DayOfWeek.Saturday, Ten, Five).Value;

        Result result = barber.SetSchedule([shift], ShopHours);

        Assert.True(result.IsSuccess);
        Assert.Equal(shift, barber.HoursOn(Saturday));
        Assert.Null(barber.HoursOn(Sunday));
    }

    /// <summary>Un horario que termina después del cierre, o en un día cerrado, se rechaza.</summary>
    [Fact]
    public void SetSchedule_OutsideShopHours_Fails()
    {
        Barber barber = Barber.Create(BarberName).Value;
        DailyHours late = DailyHours.Create(DayOfWeek.Saturday, Ten, Nine30Night).Value;
        DailyHours closedDay = DailyHours.Create(DayOfWeek.Sunday, Ten, Five).Value;

        Assert.Equal(StaffErrors.OutsideShopHours, barber.SetSchedule([late], ShopHours).Error);
        Assert.Equal(StaffErrors.OutsideShopHours, barber.SetSchedule([closedDay], ShopHours).Error);
        Assert.Empty(barber.Schedule);
    }

    /// <summary>En un día de ausencia el barbero no trabaja aunque tenga horario.</summary>
    [Fact]
    public void HoursOn_DuringTimeOff_ReturnsNull()
    {
        Barber barber = Barber.Create(BarberName).Value;
        barber.SetSchedule([DailyHours.Create(DayOfWeek.Saturday, Ten, Five).Value], ShopHours);

        TimeOff timeOff = barber.AddTimeOff(Saturday, Sunday, "Viaje").Value;

        Assert.Null(barber.HoursOn(Saturday));
        Assert.True(barber.RemoveTimeOff(timeOff.Id).IsSuccess);
        Assert.NotNull(barber.HoursOn(Saturday));
    }

    /// <summary>Una ausencia que termina antes de empezar se rechaza.</summary>
    [Fact]
    public void AddTimeOff_EndsBeforeStart_Fails()
    {
        Barber barber = Barber.Create(BarberName).Value;

        Assert.Equal(StaffErrors.InvalidTimeOff, barber.AddTimeOff(Sunday, Saturday, null).Error);
    }

    /// <summary>Los servicios asignados se guardan sin repetir.</summary>
    [Fact]
    public void AssignServices_Repeated_KeepsUnique()
    {
        Barber barber = Barber.Create(BarberName).Value;
        Guid cut = Guid.NewGuid();

        barber.AssignServices([cut, cut]);

        Assert.Single(barber.ServiceIds);
        Assert.True(barber.Offers(cut));
    }
}
