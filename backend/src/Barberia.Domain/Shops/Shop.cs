using Barberia.Domain.Common;

namespace Barberia.Domain.Shops;

/// <summary>La barbería: sus datos, su horario de atención, sus días cerrados y sus reglas de reserva.</summary>
public sealed class Shop : Entity
{
    private readonly List<DailyHours> _openingHours = [];
    private readonly List<ClosedDay> _closedDays = [];

    /// <summary>Constructor para la base de datos.</summary>
    private Shop()
    {
    }

    /// <summary>Nombre que se muestra en la página de reservas.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Dirección del local.</summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>Teléfono o WhatsApp del local.</summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>Mensaje corto que ve el cliente al reservar.</summary>
    public string WelcomeMessage { get; private set; } = string.Empty;

    /// <summary>Reglas de reserva vigentes.</summary>
    public BookingRules Rules { get; private set; } = BookingRules.Default;

    /// <summary>Horario de atención por día de la semana; un día sin horario está cerrado.</summary>
    public IReadOnlyList<DailyHours> OpeningHours => _openingHours;

    /// <summary>Fechas puntuales en las que no se abre.</summary>
    public IReadOnlyList<ClosedDay> ClosedDays => _closedDays;

    /// <summary>Crea la barbería con su nombre y las reglas por defecto.</summary>
    public static Result<Shop> Create(string? name)
    {
        Result<string> validName = Text.Required(name, Limits.NameMaxLength, ShopErrors.InvalidName);
        if (validName.IsFailure)
        {
            return validName.Error!;
        }

        return new Shop { Name = validName.Value };
    }

    /// <summary>Actualiza los datos que ve el cliente.</summary>
    public Result UpdateProfile(string? name, string? address, string? phone, string? welcomeMessage)
    {
        Result<string> validName = Text.Required(name, Limits.NameMaxLength, ShopErrors.InvalidName);
        Result<string> validAddress = Text.Optional(address, Limits.AddressMaxLength, ShopErrors.InvalidAddress);
        Result<string> validPhone = Text.Optional(phone, Limits.PhoneMaxLength, ShopErrors.InvalidPhone);
        Result<string> validWelcome = Text.Optional(welcomeMessage, Limits.WelcomeMaxLength, ShopErrors.InvalidWelcome);
        Error? error = validName.Error ?? validAddress.Error ?? validPhone.Error ?? validWelcome.Error;
        if (error is not null)
        {
            return error;
        }

        Name = validName.Value;
        Address = validAddress.Value;
        Phone = validPhone.Value;
        WelcomeMessage = validWelcome.Value;
        return Result.Success();
    }

    /// <summary>Reemplaza el horario semanal de atención.</summary>
    public Result SetOpeningHours(IReadOnlyCollection<DailyHours> hours)
    {
        Result unique = DailyHours.EnsureOnePerDay(hours);
        if (unique.IsFailure)
        {
            return unique;
        }

        _openingHours.Clear();
        _openingHours.AddRange(hours.OrderBy(item => item.Day).Select(item => item with { }));
        return Result.Success();
    }

    /// <summary>Marca una fecha como cerrada.</summary>
    public Result AddClosedDay(DateOnly date, string? reason)
    {
        Result<string> validReason = Text.Optional(reason, Limits.ReasonMaxLength, ShopErrors.InvalidReason);
        if (validReason.IsFailure)
        {
            return validReason;
        }

        if (_closedDays.Any(day => day.Date == date))
        {
            return ShopErrors.ClosedDayExists;
        }

        _closedDays.Add(new ClosedDay(date, validReason.Value));
        return Result.Success();
    }

    /// <summary>Quita una fecha cerrada.</summary>
    public Result RemoveClosedDay(DateOnly date)
    {
        int removed = _closedDays.RemoveAll(day => day.Date == date);
        if (removed == 0)
        {
            return ShopErrors.ClosedDayNotFound;
        }

        return Result.Success();
    }

    /// <summary>Cambia las reglas de reserva.</summary>
    public void UpdateRules(BookingRules rules)
    {
        Rules = rules;
    }

    /// <summary>Devuelve el horario de una fecha, o nulo si ese día no se abre.</summary>
    public DailyHours? HoursOn(DateOnly date)
    {
        if (_closedDays.Any(day => day.Date == date))
        {
            return null;
        }

        return _openingHours.FirstOrDefault(hours => hours.Day == date.DayOfWeek);
    }
}
