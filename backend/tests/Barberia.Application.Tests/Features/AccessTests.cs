using Barberia.Application.Features.Access;
using Barberia.Application.Features.Shops;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Access;
using Barberia.Domain.Common;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas del arranque de la instalación y del inicio de sesión.</summary>
public sealed class AccessTests
{
    private const string ShopName = "Barbería Don Lucho";
    private const string OwnerEmail = "dueno@barberia.bo";
    private const string OwnerPassword = "contrasena-segura";

    private readonly InMemoryStore _store = new();
    private readonly FakePasswordHasher _hasher = new();

    /// <summary>El primer arranque crea la barbería y el dueño; el segundo no duplica nada.</summary>
    [Fact]
    public async Task InitializeShop_TwoStarts_CreatesOnce()
    {
        InitializeShopHandler handler = new(_store, _store, _hasher, _store);
        InitializeShopCommand command = new(ShopName, OwnerEmail, OwnerPassword);

        await handler.HandleAsync(command, TestContext.Current.CancellationToken);
        Result second = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(second.IsSuccess);
        Assert.Equal(ShopName, _store.Shop!.Name);
        StaffAccount owner = Assert.Single(_store.Accounts);
        Assert.Equal(StaffRole.Owner, owner.Role);
    }

    /// <summary>Sin contraseña válida para el dueño, el arranque falla en vez de dejar el panel abierto.</summary>
    [Fact]
    public async Task InitializeShop_WeakOwnerPassword_Fails()
    {
        InitializeShopHandler handler = new(_store, _store, _hasher, _store);

        Result result = await handler.HandleAsync(new InitializeShopCommand(ShopName, OwnerEmail, "corta"), TestContext.Current.CancellationToken);

        Assert.Equal(AccessErrors.WeakPassword, result.Error);
        Assert.Empty(_store.Accounts);
    }

    /// <summary>Con las credenciales correctas se emite el pase; con otras, el mismo error sin pistas.</summary>
    [Fact]
    public async Task Login_RightAndWrongPassword_IssuesTokenOnlyWhenRight()
    {
        await new InitializeShopHandler(_store, _store, _hasher, _store).HandleAsync(new InitializeShopCommand(ShopName, OwnerEmail, OwnerPassword), TestContext.Current.CancellationToken);
        LoginHandler login = new(_store, _hasher, new FakeTokenIssuer());

        Result<LoginResult> ok = await login.HandleAsync(new LoginCommand(OwnerEmail, OwnerPassword), TestContext.Current.CancellationToken);
        Result<LoginResult> wrong = await login.HandleAsync(new LoginCommand(OwnerEmail, "otra-cosa"), TestContext.Current.CancellationToken);
        Result<LoginResult> unknown = await login.HandleAsync(new LoginCommand("nadie@barberia.bo", OwnerPassword), TestContext.Current.CancellationToken);

        Assert.Equal($"token-{OwnerEmail}", ok.Value.Token.Token);
        Assert.Equal(AccessErrors.InvalidCredentials, wrong.Error);
        Assert.Equal(AccessErrors.InvalidCredentials, unknown.Error);
    }
}
