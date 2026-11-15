using Barberia.Application.Ports;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Staff;

/// <summary>Lista el equipo: todos para el panel, solo los activos para el cliente.</summary>
public sealed class ListBarbersHandler(IBarberRepository barbers)
{
    /// <summary>Devuelve los barberos ordenados por nombre.</summary>
    public Task<IReadOnlyList<Barber>> HandleAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        return barbers.ListAsync(onlyActive, cancellationToken);
    }
}
