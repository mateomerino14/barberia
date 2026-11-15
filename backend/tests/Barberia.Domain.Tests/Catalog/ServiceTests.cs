using Barberia.Domain.Catalog;

namespace Barberia.Domain.Tests.Catalog;

/// <summary>Pruebas de los servicios y los precios.</summary>
public sealed class ServiceTests
{
    private const string ServiceName = "Degradado";
    private const int FortyFiveMinutes = 45;
    private const int FiftyBolivianos = 5000;
    private const int FiftyAndAHalf = 5050;

    /// <summary>Un servicio válido se crea activo.</summary>
    [Fact]
    public void Create_ValidData_IsActive()
    {
        Service service = Service.Create(ServiceName, FortyFiveMinutes, Money.FromCents(FiftyBolivianos).Value).Value;

        Assert.True(service.IsActive);
        Assert.Equal(FortyFiveMinutes, service.DurationMinutes);
    }

    /// <summary>La duración tiene que ir en pasos de 5 minutos y dentro del rango.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(485)]
    public void Create_InvalidDuration_Fails(int minutes)
    {
        Money price = Money.FromCents(FiftyBolivianos).Value;

        Assert.Equal(ServiceErrors.InvalidDuration, Service.Create(ServiceName, minutes, price).Error);
    }

    /// <summary>Un precio negativo no existe.</summary>
    [Fact]
    public void Money_Negative_Fails()
    {
        Assert.True(Money.FromCents(-1).IsFailure);
    }

    /// <summary>El precio se muestra en bolivianos, con centavos solo si los hay.</summary>
    [Fact]
    public void Money_ToString_ShowsBolivianos()
    {
        Assert.Equal("Bs 50", Money.FromCents(FiftyBolivianos).Value.ToString());
        Assert.Equal("Bs 50,50", Money.FromCents(FiftyAndAHalf).Value.ToString());
    }
}
