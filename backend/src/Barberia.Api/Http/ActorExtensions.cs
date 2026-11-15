using System.Security.Claims;
using Barberia.Application.Features.Agenda;
using Barberia.Domain.Access;

namespace Barberia.Api.Http;

/// <summary>Lee del pase de sesión quién está usando el panel.</summary>
public static class ActorExtensions
{
    private const string RoleClaim = "role";
    private const string BarberClaim = "barber_id";

    /// <summary>Devuelve el rol y la ficha de barbero de la sesión.</summary>
    public static StaffActor ToActor(this ClaimsPrincipal user)
    {
        StaffRole role = StaffRole.Barber;
        if (Enum.TryParse(user.FindFirstValue(RoleClaim), out StaffRole parsed))
        {
            role = parsed;
        }

        Guid? barberId = null;
        if (Guid.TryParse(user.FindFirstValue(BarberClaim), out Guid id))
        {
            barberId = id;
        }

        return new StaffActor(role, barberId);
    }
}
