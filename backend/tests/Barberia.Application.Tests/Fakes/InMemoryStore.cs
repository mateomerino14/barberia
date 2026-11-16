using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Media;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Tests.Fakes;

/// <summary>Doble de prueba que guarda todo en memoria e implementa los puertos de persistencia.</summary>
public sealed class InMemoryStore :
    IShopRepository,
    IServiceRepository,
    IBarberRepository,
    IStaffAccountRepository,
    IBookingRepository,
    IBlockRepository,
    IImageRepository,
    IPasswordResetRepository,
    IUnitOfWork
{
    /// <summary>Barbería guardada.</summary>
    public Shop? Shop { get; private set; }

    /// <summary>Servicios guardados.</summary>
    public List<Service> Services { get; } = [];

    /// <summary>Barberos guardados.</summary>
    public List<Barber> Barbers { get; } = [];

    /// <summary>Cuentas guardadas.</summary>
    public List<StaffAccount> Accounts { get; } = [];

    /// <summary>Turnos guardados.</summary>
    public List<Booking> Bookings { get; } = [];

    /// <summary>Bloqueos guardados.</summary>
    public List<Block> Blocks { get; } = [];

    /// <summary>Imágenes guardadas.</summary>
    public List<StoredImage> Images { get; } = [];

    /// <summary>Enlaces para cambiar la contraseña.</summary>
    public List<PasswordReset> Resets { get; } = [];

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

    /// <summary>Lista las cuentas vinculadas a barberos.</summary>
    public Task<IReadOnlyList<StaffAccount>> ListBarberAccountsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<StaffAccount>>([.. Accounts.Where(account => account.BarberId is not null)]);
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

    /// <summary>Busca un turno.</summary>
    Task<Booking?> IBookingRepository.GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Bookings.FirstOrDefault(booking => booking.Id == id));
    }

    /// <summary>Busca un turno por la clave de su enlace.</summary>
    public Task<Booking?> FindByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return Task.FromResult(Bookings.FirstOrDefault(booking => booking.ManageToken == token));
    }

    /// <summary>Lista los turnos que empiezan en el intervalo.</summary>
    Task<IReadOnlyList<Booking>> IBookingRepository.ListAsync(TimeRange range, Guid? barberId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Booking> list = [.. Bookings
            .Where(booking => booking.Range.Start >= range.Start && booking.Range.Start < range.End)
            .Where(booking => barberId is null || booking.BarberId == barberId)
            .OrderBy(booking => booking.Range.Start)];
        return Task.FromResult(list);
    }

    /// <summary>Lista los turnos reservados por recordar.</summary>
    public Task<IReadOnlyList<Booking>> ListPendingRemindersAsync(DateTimeOffset from, DateTimeOffset until, CancellationToken cancellationToken)
    {
        IReadOnlyList<Booking> list = [.. Bookings.Where(booking => booking.NeedsReminder && booking.Range.Start > from && booking.Range.Start <= until)];
        return Task.FromResult(list);
    }

    /// <summary>Guarda un turno.</summary>
    public void Add(Booking booking)
    {
        Bookings.Add(booking);
    }

    /// <summary>Busca un bloqueo.</summary>
    Task<Block?> IBlockRepository.GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Blocks.FirstOrDefault(block => block.Id == id));
    }

    /// <summary>Lista los bloqueos que empiezan en el intervalo.</summary>
    Task<IReadOnlyList<Block>> IBlockRepository.ListAsync(TimeRange range, Guid? barberId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Block> list = [.. Blocks
            .Where(block => block.Range.Start >= range.Start && block.Range.Start < range.End)
            .Where(block => barberId is null || block.BarberId == barberId)];
        return Task.FromResult(list);
    }

    /// <summary>Guarda un bloqueo.</summary>
    public void Add(Block block)
    {
        Blocks.Add(block);
    }

    /// <summary>Quita un bloqueo.</summary>
    public void Remove(Block block)
    {
        Blocks.Remove(block);
    }

    /// <summary>Busca una imagen.</summary>
    Task<StoredImage?> IImageRepository.GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Images.FirstOrDefault(image => image.Id == id));
    }

    /// <summary>Indica si existen todas las imágenes.</summary>
    Task<bool> IImageRepository.AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        return Task.FromResult(ids.All(id => Images.Any(image => image.Id == id)));
    }

    /// <summary>Guarda una imagen.</summary>
    public void Add(StoredImage image)
    {
        Images.Add(image);
    }

    /// <summary>Borra imágenes.</summary>
    public Task RemoveAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        Images.RemoveAll(image => ids.Contains(image.Id));
        return Task.CompletedTask;
    }

    /// <summary>Busca un enlace por su huella.</summary>
    public Task<PasswordReset?> FindByFingerprintAsync(string fingerprint, CancellationToken cancellationToken)
    {
        return Task.FromResult(Resets.FirstOrDefault(reset => reset.TokenHash == fingerprint));
    }

    /// <summary>Lista los enlaces sin usar de una cuenta.</summary>
    public Task<IReadOnlyList<PasswordReset>> ListUnusedAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<PasswordReset>>([.. Resets.Where(reset => reset.AccountId == accountId && reset.UsedAt is null)]);
    }

    /// <summary>Guarda un enlace.</summary>
    public void Add(PasswordReset reset)
    {
        Resets.Add(reset);
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
