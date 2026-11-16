using Barberia.Domain.Access;

namespace Barberia.Domain.Tests.Access;

/// <summary>Pruebas del enlace para cambiar la contraseña.</summary>
public sealed class PasswordResetTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Solo se guarda la huella de la clave, y con la clave se encuentra.</summary>
    [Fact]
    public void Issue_StoresOnlyFingerprint()
    {
        PasswordResetLink link = PasswordReset.Issue(Guid.NewGuid(), Now);

        Assert.NotEqual(link.Token, link.Reset.TokenHash);
        Assert.Equal(PasswordReset.Fingerprint(link.Token), link.Reset.TokenHash);
    }

    /// <summary>Sirve durante 30 minutos y una sola vez.</summary>
    [Fact]
    public void IsUsable_UntilExpiryAndOnlyOnce()
    {
        PasswordReset reset = PasswordReset.Issue(Guid.NewGuid(), Now).Reset;

        bool before = reset.IsUsable(Now.AddMinutes(PasswordReset.LifetimeMinutes - 1));
        bool expired = reset.IsUsable(Now.AddMinutes(PasswordReset.LifetimeMinutes));
        reset.MarkUsed(Now);

        Assert.True(before);
        Assert.False(expired);
        Assert.False(reset.IsUsable(Now));
    }
}
