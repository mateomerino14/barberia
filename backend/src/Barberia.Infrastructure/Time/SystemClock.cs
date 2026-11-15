using Barberia.Application.Ports;
using Barberia.Domain.Common;

namespace Barberia.Infrastructure.Time;

/// <summary>Adaptador del reloj que lee la hora del sistema y la pasa a la hora de la barbería.</summary>
public sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    /// <summary>Hora actual en la zona de la barbería.</summary>
    public DateTimeOffset Now => BusinessTime.ToLocal(timeProvider.GetUtcNow());
}
