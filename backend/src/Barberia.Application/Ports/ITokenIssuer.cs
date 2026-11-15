using Barberia.Domain.Access;

namespace Barberia.Application.Ports;

/// <summary>Puerto que emite el pase de sesión del panel.</summary>
public interface ITokenIssuer
{
    /// <summary>Emite un pase para la cuenta, válido por una jornada.</summary>
    IssuedToken Issue(StaffAccount account);
}

/// <summary>Pase de sesión emitido y hasta cuándo vale.</summary>
public sealed record IssuedToken(string Token, DateTimeOffset ExpiresAt);
