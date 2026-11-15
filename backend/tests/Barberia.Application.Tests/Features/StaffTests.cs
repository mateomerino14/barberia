using Barberia.Application.Features.Staff;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Barberia.Domain.Staff;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas de los casos de uso del equipo.</summary>
public sealed class StaffTests
{
    private const string BarberName = "Marco";
    private const string BarberEmail = "marco@barberia.bo";
    private const string BarberPassword = "barbero-seguro";

    private readonly InMemoryStore _store = new();

    /// <summary>Crear un barbero con cuenta guarda ambos y los confirma juntos.</summary>
    [Fact]
    public async Task CreateBarber_WithAccount_SavesBothOnce()
    {
        CreateBarberHandler handler = new(_store, _store, _store, new FakePasswordHasher(), _store);

        Result<Barber> barber = await handler.HandleAsync(new SaveBarberCommand(BarberName, true, []), new BarberAccountInput(BarberEmail, BarberPassword), TestContext.Current.CancellationToken);

        StaffAccount account = Assert.Single(_store.Accounts);
        Assert.Equal(barber.Value.Id, account.BarberId);
        Assert.Equal(1, _store.Saves);
    }

    /// <summary>Si el correo ya está en uso, no se crea ni el barbero ni la cuenta.</summary>
    [Fact]
    public async Task CreateBarber_EmailTaken_SavesNothing()
    {
        CreateBarberHandler handler = new(_store, _store, _store, new FakePasswordHasher(), _store);
        BarberAccountInput account = new(BarberEmail, BarberPassword);
        await handler.HandleAsync(new SaveBarberCommand(BarberName, true, []), account, TestContext.Current.CancellationToken);

        Result<Barber> second = await handler.HandleAsync(new SaveBarberCommand("Otro", true, []), account, TestContext.Current.CancellationToken);

        Assert.Equal(AccessErrors.EmailTaken, second.Error);
        Assert.Single(_store.Barbers);
    }
}
