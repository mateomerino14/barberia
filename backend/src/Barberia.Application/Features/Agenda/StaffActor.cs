using Barberia.Domain.Access;
using Barberia.Domain.Common;

namespace Barberia.Application.Features.Agenda;

/// <summary>Quién usa el panel: el dueño ve todo, un barbero solo su propia agenda.</summary>
public sealed record StaffActor(StaffRole Role, Guid? BarberId)
{
    /// <summary>Error cuando un barbero intenta tocar la agenda de otro.</summary>
    public static readonly Error OtherBarber = Error.Forbidden("agenda.other_barber", "Solo puedes manejar tu propia agenda.");

    /// <summary>Indica si puede ver o cambiar la agenda de ese barbero.</summary>
    public bool CanManage(Guid barberId)
    {
        return Role == StaffRole.Owner || BarberId == barberId;
    }

    /// <summary>Barbero que se muestra: el pedido si es el dueño, o siempre el propio si es barbero.</summary>
    public Guid? Scope(Guid? requested)
    {
        if (Role == StaffRole.Owner)
        {
            return requested;
        }

        // Un barbero sin ficha no ve ninguna agenda, nunca la de todos.
        return BarberId ?? Guid.Empty;
    }
}
