using Barberia.Domain.Access;

namespace Barberia.Domain.Tests.Access;

/// <summary>Pruebas de las cuentas del panel.</summary>
public sealed class StaffAccountTests
{
    private const string Email = "Lucho@Barberia.bo";
    private const string Hash = "hash";

    /// <summary>La cuenta del dueño se crea con el correo en minúsculas.</summary>
    [Fact]
    public void Create_Owner_NormalizesEmail()
    {
        StaffAccount account = StaffAccount.Create(Email, Hash, StaffRole.Owner, null).Value;

        Assert.Equal("lucho@barberia.bo", account.Email);
    }

    /// <summary>Un barbero sin ficha vinculada no puede tener cuenta.</summary>
    [Fact]
    public void Create_BarberWithoutProfile_Fails()
    {
        Assert.Equal(AccessErrors.BarberRequired, StaffAccount.Create(Email, Hash, StaffRole.Barber, null).Error);
    }

    /// <summary>Las contraseñas cortas o demasiado largas se rechazan.</summary>
    [Theory]
    [InlineData("corta")]
    [InlineData("")]
    public void ValidatePassword_TooShort_Fails(string password)
    {
        Assert.True(StaffAccount.ValidatePassword(password).IsFailure);
    }
}
