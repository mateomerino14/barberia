using Barberia.Api.Contracts;
using Barberia.Domain.Common;

namespace Barberia.Api.Tests.Support;

/// <summary>Datos de una prueba de agenda: un servicio propio y un barbero que lo hace toda la semana.</summary>
public sealed record Scenario(Guid ServiceId, Guid BarberId)
{
    /// <summary>Duración del servicio de prueba.</summary>
    public const int ServiceMinutes = 45;

    private const int FiftyBolivianos = 5000;
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Eight = new(20, 0);
    private static readonly BookingRulesDto DefaultRules = new(60, 14, 120);

    /// <summary>Abre la barbería toda la semana y crea un servicio y un barbero nuevos, con cuenta si se pide.</summary>
    public static async Task<Scenario> CreateAsync(ApiFactory factory, string? barberEmail = null, string? barberPassword = null)
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        List<HoursDto> week = [.. Enum.GetValues<DayOfWeek>().Select(day => new HoursDto(day, Nine, Eight))];
        await owner.PutJsonAsync("/api/shop/hours", new SetHoursRequest(week));
        await owner.PutJsonAsync("/api/shop", new UpdateShopRequest(ApiFactory.ShopName, "", "", "", DefaultRules));
        ServiceResponse service = await (await owner.PostJsonAsync("/api/services", new SaveServiceRequest($"Servicio {Guid.NewGuid():N}", ServiceMinutes, FiftyBolivianos))).ReadAsync<ServiceResponse>();
        BarberAccountRequest? account = null;
        if (barberEmail is not null)
        {
            account = new BarberAccountRequest(barberEmail, barberPassword);
        }

        BarberResponse barber = await (await owner.PostJsonAsync("/api/barbers", new SaveBarberRequest("Barbero", [service.Id], true, account))).ReadAsync<BarberResponse>();
        await owner.PutJsonAsync($"/api/barbers/{barber.Id}/schedule", new SetHoursRequest(week));
        return new Scenario(service.Id, barber.Id);
    }

    /// <summary>Hora de La Paz con su desfase, como la manda la web.</summary>
    public static DateTimeOffset Local(DateOnly date, TimeOnly time)
    {
        return BusinessTime.ToLocal(BusinessTime.At(date, time));
    }
}
