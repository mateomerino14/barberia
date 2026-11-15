using Barberia.Application.Ports;
using Barberia.Domain.Common;

namespace Barberia.Application.Tests.Fakes;

/// <summary>Reloj de prueba: martes 17 de noviembre de 2026 a las 08:00 en La Paz, movible a mano.</summary>
public sealed class FakeClock : IClock
{
    /// <summary>Día de las pruebas, un martes en que la barbería de demostración abre.</summary>
    public static readonly DateOnly Today = new(2026, 11, 17);

    private static readonly TimeOnly EightInTheMorning = new(8, 0);

    /// <summary>Hora actual de la prueba.</summary>
    public DateTimeOffset Now { get; set; } = BusinessTime.ToLocal(BusinessTime.At(Today, EightInTheMorning));
}

/// <summary>Canal de avisos de prueba que guarda lo que se habría enviado.</summary>
public sealed class CapturingNotifier : INotifier
{
    /// <summary>Avisos recibidos, en orden.</summary>
    public List<BookingNotice> Notices { get; } = [];

    /// <summary>Guarda el aviso.</summary>
    public Task NotifyAsync(BookingNotice notice, CancellationToken cancellationToken)
    {
        Notices.Add(notice);
        return Task.CompletedTask;
    }
}
