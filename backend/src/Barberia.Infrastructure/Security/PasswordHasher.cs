using Barberia.Application.Ports;
using Microsoft.AspNetCore.Identity;

namespace Barberia.Infrastructure.Security;

/// <summary>Adaptador que protege contraseñas con el algoritmo de ASP.NET Core Identity.</summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private static readonly object Owner = new();
    private readonly PasswordHasher<object> _hasher = new();

    /// <summary>Devuelve la contraseña protegida.</summary>
    public string Hash(string password)
    {
        return _hasher.HashPassword(Owner, password);
    }

    /// <summary>Indica si la contraseña coincide con la protegida.</summary>
    public bool Verify(string passwordHash, string password)
    {
        PasswordVerificationResult result = _hasher.VerifyHashedPassword(Owner, passwordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
