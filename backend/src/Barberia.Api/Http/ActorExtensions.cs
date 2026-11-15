using System.Security.Claims;
using Barberia.Application.Features.Agenda;
using Barberia.Domain.Access;

namespace Barberia.Api.Http;

/// <summary>Lee del pase de sesión quién está usando el panel.</summary>
public static class ActorExtensions
{
    private const string RoleClaim = "role";
    private const string BarberClaim = "barber_id";
    private const string AccountClaim = "sub";

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

    /// <summary>Devuelve el id de la cuenta de la sesión, o vacío si el pase no lo trae.</summary>
    public static Guid AccountId(this ClaimsPrincipal user)
    {
        if (Guid.TryParse(user.FindFirstValue(AccountClaim), out Guid id))
        {
            return id;
        }

        return Guid.Empty;
    }
}
