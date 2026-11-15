using Barberia.Application.Ports;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Shops;

/// <summary>Carga datos de ejemplo para mostrar el sistema: horario, servicios y tres barberos.</summary>
public sealed class SeedDemoHandler(IShopRepository shops, IServiceRepository services, IBarberRepository barbers, IUnitOfWork unitOfWork)
{
    private const string Address = "Av. 20 de Octubre 2045, Sopocachi, La Paz";
    private const string Phone = "71234567";
    private const string Welcome = "Tu turno, sin filas ni mensajes de ida y vuelta.";
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly Two = new(14, 0);
    private static readonly TimeOnly Seven = new(19, 0);
    private static readonly TimeOnly Eight = new(20, 0);
    private static readonly DayOfWeek[] Weekdays = [DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];
    private static readonly (string Name, int Minutes, int Cents)[] Catalog =
    [
        ("Corte clásico", 30, 4000),
        ("Degradado", 45, 5000),
        ("Barba", 20, 3000),
        ("Corte + barba", 60, 7000),
    ];

    private static readonly string[] Team = ["Lucho", "Marco", "Rodri"];

    /// <summary>Carga los datos solo si la barbería todavía no tiene servicios.</summary>
    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null || (await services.ListAsync(false, cancellationToken)).Count > 0)
        {
            return;
        }

        shop.UpdateProfile(shop.Name, Address, Phone, Welcome);
        List<DailyHours> hours = [.. Weekdays.Select(day => DailyHours.Create(day, Nine, Eight).Value)];
        hours.Add(DailyHours.Create(DayOfWeek.Saturday, Nine, Eight).Value);
        hours.Add(DailyHours.Create(DayOfWeek.Sunday, Ten, Two).Value);
        shop.SetOpeningHours(hours);

        List<Service> catalog = [.. Catalog.Select(item => Service.Create(item.Name, item.Minutes, Money.FromCents(item.Cents).Value).Value)];
        catalog.ForEach(services.Add);
        List<DailyHours> shift = [.. Weekdays.Select(day => DailyHours.Create(day, Nine, Seven).Value)];
        shift.Add(DailyHours.Create(DayOfWeek.Saturday, Nine, Eight).Value);
        shift.Add(DailyHours.Create(DayOfWeek.Sunday, Ten, Two).Value);
        foreach (string name in Team)
        {
            Barber barber = Barber.Create(name).Value;
            barber.AssignServices(catalog.Select(service => service.Id));
            barber.SetSchedule(shift, hours);
            barbers.Add(barber);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
