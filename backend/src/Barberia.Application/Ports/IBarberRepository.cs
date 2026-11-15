using Barberia.Domain.Staff;

namespace Barberia.Application.Ports;

/// <summary>Puerto de persistencia del equipo de barberos.</summary>
public interface IBarberRepository
{
    /// <summary>Busca un barbero por su Id.</summary>
    Task<Barber?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Lista los barberos ordenados por nombre, solo los activos si se pide.</summary>
    Task<IReadOnlyList<Barber>> ListAsync(bool onlyActive, CancellationToken cancellationToken);

    /// <summary>Agrega un barbero.</summary>
    void Add(Barber barber);
}
