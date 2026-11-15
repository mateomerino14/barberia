using Barberia.Domain.Common;

namespace Barberia.Domain.Tests.Common;

/// <summary>Pruebas de la zona horaria de la barbería.</summary>
public sealed class BusinessTimeTests
{
    private static readonly TimeSpan BoliviaOffset = TimeSpan.FromHours(-4);

    /// <summary>La hora de Bolivia va cuatro horas detrás de UTC todo el año.</summary>
    [Fact]
    public void ToLocal_UtcNoon_ReturnsEightInTheMorning()
    {
        DateTimeOffset noonUtc = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

        DateTimeOffset local = BusinessTime.ToLocal(noonUtc);

        Assert.Equal(BoliviaOffset, local.Offset);
        Assert.Equal(noonUtc.UtcDateTime, local.UtcDateTime);
    }
}
