using Barberia.Application.Ports;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;

namespace Barberia.Application.Features.Catalog;

/// <summary>Datos de un servicio que se crea o se edita.</summary>
public sealed record SaveServiceCommand(string? Name, int DurationMinutes, int PriceCents, bool IsActive);

/// <summary>Crea un servicio nuevo.</summary>
public sealed class CreateServiceHandler(IServiceRepository services, IUnitOfWork unitOfWork)
{
    /// <summary>Guarda el servicio si sus datos son válidos.</summary>
    public async Task<Result<Service>> HandleAsync(SaveServiceCommand command, CancellationToken cancellationToken)
    {
        Result<Money> price = Money.FromCents(command.PriceCents);
        if (price.IsFailure)
        {
            return price.Error!;
        }

        Result<Service> service = Service.Create(command.Name, command.DurationMinutes, price.Value);
        if (service.IsFailure)
        {
            return service;
        }

        service.Value.SetActive(command.IsActive);
        services.Add(service.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return service;
    }
}

/// <summary>Edita un servicio existente.</summary>
public sealed class UpdateServiceHandler(IServiceRepository services, IUnitOfWork unitOfWork)
{
    /// <summary>Guarda los cambios si el servicio existe y los datos son válidos.</summary>
    public async Task<Result<Service>> HandleAsync(Guid id, SaveServiceCommand command, CancellationToken cancellationToken)
    {
        Service? service = await services.GetAsync(id, cancellationToken);
        if (service is null)
        {
            return ServiceErrors.NotFound;
        }

        Result<Money> price = Money.FromCents(command.PriceCents);
        if (price.IsFailure)
        {
            return price.Error!;
        }

        Result update = service.Update(command.Name, command.DurationMinutes, price.Value);
        if (update.IsFailure)
        {
            return update.Error!;
        }

        service.SetActive(command.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return service;
    }
}
