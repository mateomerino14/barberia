using Barberia.Application.Features;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Api.Contracts;

/// <summary>Horario de un día en la API.</summary>
public sealed record HoursDto(DayOfWeek Day, TimeOnly Opens, TimeOnly Closes)
{
    /// <summary>Convierte el horario del dominio.</summary>
    public static HoursDto From(DailyHours hours)
    {
        return new HoursDto(hours.Day, hours.Opens, hours.Closes);
    }

    /// <summary>Convierte el horario a la entrada de los casos de uso.</summary>
    public HoursInput ToInput()
    {
        return new HoursInput(Day, Opens, Closes);
    }
}

/// <summary>Día cerrado en la API.</summary>
public sealed record ClosedDayDto(DateOnly Date, string Reason);

/// <summary>Reglas de reserva en la API.</summary>
public sealed record BookingRulesDto(int MinLeadMinutes, int MaxDaysAhead, int CancelLimitMinutes);

/// <summary>La barbería tal como la ven el cliente y el panel.</summary>
public sealed record ShopResponse(
    string Name,
    string Address,
    string Phone,
    string WelcomeMessage,
    BookingRulesDto Rules,
    ShopPalette Palette,
    IReadOnlyList<HoursDto> OpeningHours,
    IReadOnlyList<ClosedDayDto> ClosedDays)
{
    /// <summary>Convierte la barbería del dominio.</summary>
    public static ShopResponse From(Shop shop)
    {
        return new ShopResponse(
            shop.Name,
            shop.Address,
            shop.Phone,
            shop.WelcomeMessage,
            new BookingRulesDto(shop.Rules.MinLeadMinutes, shop.Rules.MaxDaysAhead, shop.Rules.CancelLimitMinutes),
            shop.Palette,
            [.. shop.OpeningHours.Select(HoursDto.From)],
            [.. shop.ClosedDays.OrderBy(day => day.Date).Select(day => new ClosedDayDto(day.Date, day.Reason))]);
    }
}

/// <summary>Datos y reglas que edita el dueño.</summary>
public sealed record UpdateShopRequest(
    string? Name,
    string? Address,
    string? Phone,
    string? WelcomeMessage,
    BookingRulesDto Rules,
    ShopPalette Palette = ShopPalette.Classic);

/// <summary>Horario semanal nuevo.</summary>
public sealed record SetHoursRequest(IReadOnlyList<HoursDto> Hours);

/// <summary>Día cerrado nuevo.</summary>
public sealed record AddClosedDayRequest(DateOnly Date, string? Reason);
