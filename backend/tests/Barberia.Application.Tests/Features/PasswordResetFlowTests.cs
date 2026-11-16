using Barberia.Application.Features.Access;
using Barberia.Application.Features.Staff;
using Barberia.Application.Ports;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Access;
using Barberia.Domain.Common;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas de recuperar la contraseña por correo con puertos falsos.</summary>
public sealed class PasswordResetFlowTests
{
    private const string NewPassword = "nueva-contrasena";
    private const string UnknownEmail = "nadie@barberia.bo";

    private readonly CapturingAccessNotifier _mail = new();

    /// <summary>Con un correo registrado llega un enlace; con uno desconocido no pasa nada, sin dar pistas.</summary>
    [Fact]
    public async Task Request_KnownAndUnknownEmail()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        StaffAccount owner = world.Store.Accounts[0];

        await RequestAsync(world, owner.Email.ToUpperInvariant());
        await RequestAsync(world, UnknownEmail);

        PasswordResetNotice notice = Assert.Single(_mail.Sent);
        Assert.Equal(owner.Email, notice.Email);
        Assert.Equal(world.Store.Shop!.Name, notice.ShopName);
        Assert.Equal(PasswordReset.Fingerprint(notice.Token), Assert.Single(world.Store.Resets).TokenHash);
    }

    /// <summary>Sin correos configurados no se crea ni se manda nada.</summary>
    [Fact]
    public async Task Request_WithoutEmail_DoesNothing()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        _mail.IsAvailable = false;

        await RequestAsync(world, world.Store.Accounts[0].Email);

        Assert.Empty(world.Store.Resets);
        Assert.Empty(_mail.Sent);
        Assert.False(new PasswordResetStatusHandler(_mail).Handle());
    }

    /// <summary>Un barbero dado de baja no recibe enlace.</summary>
    [Fact]
    public async Task Request_InactiveBarber_DoesNothing()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        await new CreateBarberHandler(world.Store, world.Store, world.Store, world.Hasher, world.Store)
            .HandleAsync(new SaveBarberCommand("Ex", false, []), new BarberAccountInput("ex@barberia.bo", "barbero-seguro"), TestContext.Current.CancellationToken);

        await RequestAsync(world, "ex@barberia.bo");

        Assert.Empty(_mail.Sent);
    }

    /// <summary>El enlace cambia la contraseña una sola vez y anula los demás enlaces de la cuenta.</summary>
    [Fact]
    public async Task Reset_WorksOnceAndVoidsOthers()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        StaffAccount owner = world.Store.Accounts[0];
        string stampBefore = owner.SessionStamp;
        await RequestAsync(world, owner.Email);
        await RequestAsync(world, owner.Email);

        Result first = await ResetAsync(world, _mail.Sent[0].Token, NewPassword);
        Result again = await ResetAsync(world, _mail.Sent[0].Token, NewPassword);
        Result other = await ResetAsync(world, _mail.Sent[1].Token, NewPassword);

        Assert.True(first.IsSuccess);
        Assert.True(world.Hasher.Verify(owner.PasswordHash, NewPassword));
        Assert.NotEqual(stampBefore, owner.SessionStamp);
        Assert.Equal(AccessErrors.ResetLinkInvalid, again.Error);
        Assert.Equal(AccessErrors.ResetLinkInvalid, other.Error);
    }

    /// <summary>Un enlace vencido o una contraseña corta no cambian nada.</summary>
    [Fact]
    public async Task Reset_ExpiredOrWeak_Fails()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        await RequestAsync(world, world.Store.Accounts[0].Email);
        string token = _mail.Sent[0].Token;

        Result weak = await ResetAsync(world, token, "corta");
        world.Clock.Now = world.Clock.Now.AddMinutes(PasswordReset.LifetimeMinutes);
        Result expired = await ResetAsync(world, token, NewPassword);
        Result empty = await ResetAsync(world, "", NewPassword);

        Assert.Equal(AccessErrors.WeakPassword, weak.Error);
        Assert.Equal(AccessErrors.ResetLinkInvalid, expired.Error);
        Assert.Equal(AccessErrors.ResetLinkInvalid, empty.Error);
    }

    /// <summary>Pide el enlace para un correo.</summary>
    private Task RequestAsync(DemoWorld world, string email)
    {
        RequestPasswordResetHandler handler = new(world.Store, world.Store, world.Store, world.Store, _mail, world.Clock, world.Store);
        return handler.HandleAsync(email, TestContext.Current.CancellationToken);
    }

    /// <summary>Usa un enlace con la contraseña nueva.</summary>
    private static Task<Result> ResetAsync(DemoWorld world, string token, string password)
    {
        ResetPasswordHandler handler = new(world.Store, world.Store, world.Hasher, world.Clock, world.Store);
        return handler.HandleAsync(new ResetPasswordCommand(token, password), TestContext.Current.CancellationToken);
    }
}
