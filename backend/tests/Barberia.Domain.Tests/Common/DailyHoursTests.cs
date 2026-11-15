using Barberia.Domain.Common;

namespace Barberia.Domain.Tests.Common;

/// <summary>Pruebas del horario de un día.</summary>
public sealed class DailyHoursTests
{
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Noon = new(12, 0);
    private static readonly TimeOnly Eight = new(20, 0);

    /// <summary>Un horario que cierra antes de abrir no se puede crear.</summary>
    [Fact]
    public void Create_ClosesBeforeOpening_Fails()
    {
        Result<DailyHours> hours = DailyHours.Create(DayOfWeek.Monday, Noon, Nine);

        Assert.True(hours.IsFailure);
    }

    /// <summary>Un horario cubre a otro del mismo día que cabe adentro, y no a uno de otro día.</summary>
    [Fact]
    public void Covers_InnerRangeSameDay_IsTrueOnlyForThatDay()
    {
        DailyHours shop = DailyHours.Create(DayOfWeek.Tuesday, Nine, Eight).Value;
        DailyHours inner = DailyHours.Create(DayOfWeek.Tuesday, Nine, Noon).Value;
        DailyHours otherDay = DailyHours.Create(DayOfWeek.Wednesday, Nine, Noon).Value;

        Assert.True(shop.Covers(inner));
        Assert.False(shop.Covers(otherDay));
        Assert.False(inner.Covers(shop));
    }

    /// <summary>Una lista con dos horarios para el mismo día no es válida.</summary>
    [Fact]
    public void EnsureOnePerDay_RepeatedDay_Fails()
    {
        DailyHours morning = DailyHours.Create(DayOfWeek.Friday, Nine, Noon).Value;
        DailyHours evening = DailyHours.Create(DayOfWeek.Friday, Noon, Eight).Value;

        Result result = DailyHours.EnsureOnePerDay([morning, evening]);

        Assert.True(result.IsFailure);
    }
}
