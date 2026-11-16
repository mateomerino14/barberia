namespace Barberia.Application.Ports;

/// <summary>Correo con el enlace para crear una contraseña nueva.</summary>
public sealed record PasswordResetNotice(string Email, string ShopName, string Token, DateTimeOffset ExpiresAt);

/// <summary>Puerto que manda los correos de acceso al panel.</summary>
public interface IAccessNotifier
{
    /// <summary>Indica si hay un canal configurado para mandar estos correos.</summary>
    bool IsAvailable { get; }

    /// <summary>Manda el enlace para crear una contraseña nueva.</summary>
    Task SendPasswordResetAsync(PasswordResetNotice notice, CancellationToken cancellationToken);
}
