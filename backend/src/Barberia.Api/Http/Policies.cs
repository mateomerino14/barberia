using Barberia.Domain.Access;

namespace Barberia.Api.Http;

/// <summary>Políticas de autorización del panel.</summary>
public static class Policies
{
    /// <summary>Solo el dueño.</summary>
    public const string Owner = "owner";

    /// <summary>Cualquier cuenta del equipo con sesión abierta.</summary>
    public const string Staff = "staff";

    /// <summary>Registra las políticas del panel.</summary>
    public static IServiceCollection AddPanelPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Owner, policy => policy.RequireRole(nameof(StaffRole.Owner)))
            .AddPolicy(Staff, policy => policy.RequireAuthenticatedUser());
        return services;
    }
}
