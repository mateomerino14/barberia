namespace Barberia.Api.Contracts;

/// <summary>Credenciales para entrar al panel.</summary>
public sealed record LoginRequest(string? Email, string? Password);

/// <summary>Sesión abierta.</summary>
public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAt, string Email, string Role, Guid? BarberId);
