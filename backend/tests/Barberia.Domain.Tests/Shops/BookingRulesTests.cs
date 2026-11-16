using Barberia.Domain.Shops;

namespace Barberia.Domain.Tests.Shops;

/// <summary>Pruebas de las reglas de reserva.</summary>
public sealed class BookingRulesTests
{
    private const int OneHour = 60;
    private const int TwoWeeks = 14;
    private const int TwoHours = 120;

    /// <summary>Reglas dentro de los rangos se crean con sus valores.</summary>
    [Fact]
    public void Create_ValidValues_KeepsThem()
    {
        BookingRules rules = BookingRules.Create(OneHour, TwoWeeks, TwoHours, BookingRules.DefaultSlotStepMinutes).Value;

        Assert.Equal(OneHour, rules.MinLeadMinutes);
        Assert.Equal(TwoWeeks, rules.MaxDaysAhead);
        Assert.Equal(TwoHours, rules.CancelLimitMinutes);
    }

    /// <summary>Valores fuera de rango se rechazan.</summary>
    [Theory]
    [InlineData(-1, 14, 120)]
    [InlineData(60, 0, 120)]
    [InlineData(60, 91, 120)]
    [InlineData(60, 14, -5)]
    public void Create_OutOfRange_Fails(int minLead, int maxDays, int cancelLimit)
    {
        Assert.True(BookingRules.Create(minLead, maxDays, cancelLimit, BookingRules.DefaultSlotStepMinutes).IsFailure);
    }

    /// <summary>Los turnos solo pueden empezar cada 15, 20, 30 o 60 minutos.</summary>
    [Theory]
    [InlineData(15, true)]
    [InlineData(20, true)]
    [InlineData(30, true)]
    [InlineData(60, true)]
    [InlineData(0, false)]
    [InlineData(25, false)]
    public void Create_SlotStep_OnlyAllowedValues(int step, bool valid)
    {
        Assert.Equal(valid, BookingRules.Create(OneHour, TwoWeeks, TwoHours, step).IsSuccess);
    }
}
