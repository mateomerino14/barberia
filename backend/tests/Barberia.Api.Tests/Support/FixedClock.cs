using Barberia.Application.Ports;
using Barberia.Domain.Common;

namespace Barberia.Api.Tests.Support;

/// <summary>Reloj de prueba detenido el lunes 16 de noviembre de 2026 a las 8 de la mañana en La Paz.</summary>
public sealed class FixedClock : IClock
{
    /// <summary>Fecha en la que está detenido el reloj.</summary>
    public static readonly DateOnly Today = new(2026, 11, 16);

    private static readonly TimeOnly EightInTheMorning = new(8, 0);

    /// <summary>Hora actual fija.</summary>
    public DateTimeOffset Now => BusinessTime.ToLocal(BusinessTime.At(Today, EightInTheMorning));
}
