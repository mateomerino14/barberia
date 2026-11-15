namespace Barberia.Infrastructure.Notifications;

/// <summary>Configuración de los recordatorios, leída de la sección "Reminders".</summary>
public sealed class ReminderOptions
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Section = "Reminders";

    /// <summary>Cuántas horas antes del turno se avisa; 0 apaga los recordatorios.</summary>
    public int HoursBefore { get; init; } = 3;

    /// <summary>Cada cuántos minutos se revisa si hay turnos por recordar.</summary>
    public int IntervalMinutes { get; init; } = 5;
}
