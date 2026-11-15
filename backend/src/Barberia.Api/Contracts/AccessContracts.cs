namespace Barberia.Api.Contracts;

/// <summary>Credenciales para entrar al panel.</summary>
public sealed record LoginRequest(string? Email, string? Password);

/// <summary>Sesión abierta.</summary>
public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAt, string Email, string Role, Guid? BarberId);

/// <summary>Cambio de la propia contraseña.</summary>
public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);

/// <summary>Acceso al panel de un barbero: correo (solo para crear la cuenta) y contraseña nueva.</summary>
public sealed record BarberAccessRequest(string? Email, string? Password);

/// <summary>Barbero con acceso al panel y su correo.</summary>
public sealed record BarberAccessResponse(Guid BarberId, string Email);
