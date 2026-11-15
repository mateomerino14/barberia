using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Domain.Tests.Shops;

/// <summary>Pruebas de la barbería, su horario y sus días cerrados.</summary>
public sealed class ShopTests
{
    private const string ShopName = "Barbería Don Lucho";
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Eight = new(20, 0);
    private static readonly DateOnly Saturday = new(2026, 11, 14);
    private static readonly DateOnly Monday = new(2026, 11, 16);

    /// <summary>Una barbería sin nombre no se puede crear.</summary>
    [Fact]
    public void Create_EmptyName_Fails()
    {
        Assert.True(Shop.Create("   ").IsFailure);
    }

    /// <summary>La barbería nueva empieza con las reglas por defecto y sin horario.</summary>
    [Fact]
    public void Create_ValidName_StartsWithDefaultRulesAndClosed()
    {
        Shop shop = Shop.Create(ShopName).Value;

        Assert.Equal(BookingRules.Default, shop.Rules);
        Assert.Null(shop.HoursOn(Saturday));
    }

    /// <summary>El horario de una fecha sale de su día de la semana, y un día sin horario está cerrado.</summary>
    [Fact]
    public void HoursOn_DayWithAndWithoutHours_ReturnsHoursOrNull()
    {
        Shop shop = Shop.Create(ShopName).Value;
        DailyHours saturdayHours = DailyHours.Create(DayOfWeek.Saturday, Nine, Eight).Value;
        shop.SetOpeningHours([saturdayHours]);

        Assert.Equal(saturdayHours, shop.HoursOn(Saturday));
        Assert.Null(shop.HoursOn(Monday));
    }

    /// <summary>Un día marcado como cerrado no abre aunque su día de la semana tenga horario.</summary>
    [Fact]
    public void HoursOn_ClosedDay_ReturnsNull()
    {
        Shop shop = Shop.Create(ShopName).Value;
        shop.SetOpeningHours([DailyHours.Create(DayOfWeek.Saturday, Nine, Eight).Value]);

        shop.AddClosedDay(Saturday, "Feriado");

        Assert.Null(shop.HoursOn(Saturday));
    }

    /// <summary>No se puede marcar dos veces el mismo día ni quitar uno que no estaba.</summary>
    [Fact]
    public void ClosedDays_DuplicateOrMissing_Fail()
    {
        Shop shop = Shop.Create(ShopName).Value;
        shop.AddClosedDay(Saturday, null);

        Assert.Equal(ShopErrors.ClosedDayExists, shop.AddClosedDay(Saturday, null).Error);
        Assert.Equal(ShopErrors.ClosedDayNotFound, shop.RemoveClosedDay(Monday).Error);
        Assert.True(shop.RemoveClosedDay(Saturday).IsSuccess);
    }

    /// <summary>Los datos del perfil se guardan recortados y un nombre vacío se rechaza sin cambiar nada.</summary>
    [Fact]
    public void UpdateProfile_ValidAndInvalid_KeepsDataConsistent()
    {
        Shop shop = Shop.Create(ShopName).Value;

        shop.UpdateProfile(" Don Lucho ", "Av. 20 de Octubre", "71234567", "Bienvenido");
        Result invalid = shop.UpdateProfile("", "Otra", null, null);

        Assert.Equal("Don Lucho", shop.Name);
        Assert.Equal("Av. 20 de Octubre", shop.Address);
        Assert.Equal(ShopErrors.InvalidName, invalid.Error);
    }
}
