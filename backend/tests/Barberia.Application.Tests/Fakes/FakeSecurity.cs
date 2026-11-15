using Barberia.Application.Ports;
using Barberia.Domain.Access;

namespace Barberia.Application.Tests.Fakes;

/// <summary>Doble que "protege" contraseñas con un prefijo, para poder verificarlas en las pruebas.</summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    private const string Prefix = "hash:";

    /// <summary>Devuelve la contraseña con el prefijo.</summary>
    public string Hash(string password)
    {
        return Prefix + password;
    }

    /// <summary>Compara con el prefijo.</summary>
    public bool Verify(string passwordHash, string password)
    {
        return passwordHash == Prefix + password;
    }
}

/// <summary>Doble que emite un pase con el correo de la cuenta.</summary>
public sealed class FakeTokenIssuer : ITokenIssuer
{
    private static readonly DateTimeOffset Expiry = new(2026, 11, 15, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Emite un pase falso.</summary>
    public IssuedToken Issue(StaffAccount account)
    {
        return new IssuedToken($"token-{account.Email}", Expiry);
    }
}
