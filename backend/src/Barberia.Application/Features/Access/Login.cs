using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Common;

namespace Barberia.Application.Features.Access;

/// <summary>Datos para entrar al panel.</summary>
public sealed record LoginCommand(string? Email, string? Password);

/// <summary>Sesión abierta: el pase, su vencimiento y quién entró.</summary>
public sealed record LoginResult(IssuedToken Token, StaffAccount Account);

/// <summary>Verifica el correo y la contraseña y emite el pase de sesión.</summary>
public sealed class LoginHandler(IStaffAccountRepository accounts, IPasswordHasher hasher, ITokenIssuer issuer)
{
    /// <summary>Abre la sesión si las credenciales coinciden.</summary>
    public async Task<Result<LoginResult>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        Result<string> email = EmailAddress.Normalize(command.Email);
        if (email.IsFailure || string.IsNullOrEmpty(command.Password))
        {
            return AccessErrors.InvalidCredentials;
        }

        StaffAccount? account = await accounts.FindByEmailAsync(email.Value, cancellationToken);
        if (account is null || !hasher.Verify(account.PasswordHash, command.Password))
        {
            return AccessErrors.InvalidCredentials;
        }

        return new LoginResult(issuer.Issue(account), account);
    }
}
