namespace Barberia.Application.Ports;

/// <summary>Puerto que entrega la hora actual, para que los casos de uso se puedan probar con una hora fija.</summary>
public interface IClock
{
    /// <summary>Hora actual en la zona de la barbería.</summary>
    DateTimeOffset Now { get; }
}
