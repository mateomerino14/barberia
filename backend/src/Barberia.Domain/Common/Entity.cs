namespace Barberia.Domain.Common;

/// <summary>Base de las entidades del dominio: se identifican por su Id, no por sus datos.</summary>
public abstract class Entity
{
    /// <summary>Identificador único de la entidad.</summary>
    public Guid Id { get; protected init; } = Guid.CreateVersion7();
}
