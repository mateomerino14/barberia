namespace Barberia.Domain.Common;

/// <summary>Largos y rangos permitidos que usan varios módulos.</summary>
public static class Limits
{
    /// <summary>Largo máximo de un nombre de persona, servicio o barbería.</summary>
    public const int NameMaxLength = 80;

    /// <summary>Largo máximo de una dirección.</summary>
    public const int AddressMaxLength = 160;

    /// <summary>Largo máximo de un número de teléfono escrito.</summary>
    public const int PhoneMaxLength = 20;

    /// <summary>Largo máximo de un correo.</summary>
    public const int EmailMaxLength = 120;

    /// <summary>Largo máximo del mensaje de bienvenida de la página de reservas.</summary>
    public const int WelcomeMaxLength = 280;

    /// <summary>Largo máximo del motivo de un día cerrado, una ausencia o un bloqueo.</summary>
    public const int ReasonMaxLength = 120;

    /// <summary>Minutos que tiene un día.</summary>
    public const int MinutesPerDay = 1440;
}
