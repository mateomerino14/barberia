using Barberia.Domain.Common;

namespace Barberia.Domain.Tests.Common;

/// <summary>Pruebas de la validación de correos.</summary>
public sealed class EmailAddressTests
{
    /// <summary>Un correo válido se guarda en minúsculas y sin espacios.</summary>
    [Fact]
    public void Normalize_MixedCase_ReturnsLowerCase()
    {
        Result<string> email = EmailAddress.Normalize("  Lucho@Barberia.BO ");

        Assert.Equal("lucho@barberia.bo", email.Value);
    }

    /// <summary>Un texto sin forma de correo se rechaza.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("lucho")]
    [InlineData("lucho@")]
    [InlineData("Lucho <lucho@barberia.bo>")]
    public void Normalize_NotAnEmail_Fails(string value)
    {
        Assert.True(EmailAddress.Normalize(value).IsFailure);
    }
}
