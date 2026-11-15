using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Tests.Fakes;

/// <summary>Doble de prueba que guarda todo en memoria e implementa los puertos de persistencia.</summary>
public sealed class InMemoryStore : IShopRepository, IServiceRepository, IBarberRepository, IStaffAccountRepository, IUnitOfWork
{
    /// <summary>Barbería guardada.</summary>
    public Shop? Shop { get; private set; }

    /// <summary>Servicios guardados.</summary>
    public List<Service> Services { get; } = [];

    /// <summary>Barberos guardados.</summary>
    public List<Barber> Barbers { get; } = [];

    /// <summary>Cuentas guardadas.</summary>
    public List<StaffAccount> Accounts { get; } = [];

    /// <summary>Cuántas veces se confirmaron cambios.</summary>
    public int Saves { get; private set; }

    /// <summary>Devuelve la barbería guardada.</summary>
    public Task<Shop?> GetAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Shop);
    }

    /// <summary>Guarda la barbería.</summary>
    public void Add(Shop shop)
    {
        Shop = shop;
    }

    /// <summary>Busca un servicio.</summary>
    Task<Service?> IServiceRepository.GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Services.FirstOrDefault(service => service.Id == id));
    }

    /// <summary>Lista servicios.</summary>
    Task<IReadOnlyList<Service>> IServiceRepository.ListAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        IReadOnlyList<Service> list = [.. Services.Where(service => !onlyActive || service.IsActive)];
        return Task.FromResult(list);
    }

    /// <summary>Indica si existen todos los servicios.</summary>
    public Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        return Task.FromResult(ids.All(id => Services.Any(service => service.Id == id)));
    }

    /// <summary>Guarda un servicio.</summary>
    public void Add(Service service)
    {
        Services.Add(service);
    }

    /// <summary>Busca un barbero.</summary>
    Task<Barber?> IBarberRepository.GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Barbers.FirstOrDefault(barber => barber.Id == id));
    }

    /// <summary>Lista barberos.</summary>
    Task<IReadOnlyList<Barber>> IBarberRepository.ListAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        IReadOnlyList<Barber> list = [.. Barbers.Where(barber => !onlyActive || barber.IsActive)];
        return Task.FromResult(list);
    }

    /// <summary>Guarda un barbero.</summary>
    public void Add(Barber barber)
    {
        Barbers.Add(barber);
    }

    /// <summary>Busca una cuenta por correo.</summary>
    public Task<StaffAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return Task.FromResult(Accounts.FirstOrDefault(account => account.Email == email));
    }

    /// <summary>Busca una cuenta por su id.</summary>
    Task<StaffAccount?> IStaffAccountRepository.GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Accounts.FirstOrDefault(account => account.Id == id));
    }

    /// <summary>Busca la cuenta vinculada a un barbero.</summary>
    public Task<StaffAccount?> FindByBarberAsync(Guid barberId, CancellationToken cancellationToken)
    {
        return Task.FromResult(Accounts.FirstOrDefault(account => account.BarberId == barberId));
    }

    /// <summary>Indica si hay un dueño.</summary>
    public Task<bool> AnyOwnerAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Accounts.Any(account => account.Role == StaffRole.Owner));
    }

    /// <summary>Guarda una cuenta.</summary>
    public void Add(StaffAccount account)
    {
        Accounts.Add(account);
    }

    /// <summary>Cuenta la confirmación.</summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saves++;
        return Task.CompletedTask;
    }

    /// <summary>Cuenta la confirmación de cambios de agenda; en memoria no hay choques concurrentes.</summary>
    public Task<Result> SaveScheduleChangesAsync(CancellationToken cancellationToken)
    {
        Saves++;
        return Task.FromResult(Result.Success());
    }
}
