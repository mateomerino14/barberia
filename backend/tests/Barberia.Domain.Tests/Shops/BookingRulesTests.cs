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
        BookingRules rules = BookingRules.Create(OneHour, TwoWeeks, TwoHours).Value;

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
        Assert.True(BookingRules.Create(minLead, maxDays, cancelLimit).IsFailure);
    }
}
