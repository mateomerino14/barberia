namespace Barberia.Infrastructure.Security;

/// <summary>Configuración del pase de sesión, leída de la sección "Jwt".</summary>
public sealed class JwtOptions
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Section = "Jwt";

    /// <summary>Clave secreta para firmar, de al menos 32 caracteres.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Quién emite el pase.</summary>
    public string Issuer { get; init; } = "barberia";

    /// <summary>Para quién es el pase.</summary>
    public string Audience { get; init; } = "barberia-panel";

    /// <summary>Horas que dura una sesión.</summary>
    public int LifetimeHours { get; init; } = 12;
}
