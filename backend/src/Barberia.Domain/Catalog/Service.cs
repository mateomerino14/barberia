using Barberia.Domain.Common;

namespace Barberia.Domain.Catalog;

/// <summary>Servicio que ofrece la barbería, con su duración y su precio.</summary>
public sealed class Service : Entity
{
    private const int DurationStepMinutes = 5;
    private const int MaxDurationMinutes = 480;

    /// <summary>Constructor para la base de datos.</summary>
    private Service()
    {
    }

    /// <summary>Nombre que ve el cliente, por ejemplo "Degradado".</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Cuántos minutos ocupa en la agenda.</summary>
    public int DurationMinutes { get; private set; }

    /// <summary>Precio del servicio.</summary>
    public Money Price { get; private set; }

    /// <summary>Indica si se puede reservar; uno inactivo se oculta sin borrarse.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Crea un servicio activo.</summary>
    public static Result<Service> Create(string? name, int durationMinutes, Money price)
    {
        Service service = new();
        Result update = service.Update(name, durationMinutes, price);
        if (update.IsFailure)
        {
            return update.Error!;
        }

        return service;
    }

    /// <summary>Cambia el nombre, la duración y el precio.</summary>
    public Result Update(string? name, int durationMinutes, Money price)
    {
        Result<string> validName = Text.Required(name, Limits.NameMaxLength, ServiceErrors.InvalidName);
        if (validName.IsFailure)
        {
            return validName;
        }

        if (durationMinutes < DurationStepMinutes || durationMinutes > MaxDurationMinutes
            || durationMinutes % DurationStepMinutes != 0)
        {
            return ServiceErrors.InvalidDuration;
        }

        Name = validName.Value;
        DurationMinutes = durationMinutes;
        Price = price;
        return Result.Success();
    }

    /// <summary>Activa u oculta el servicio.</summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
