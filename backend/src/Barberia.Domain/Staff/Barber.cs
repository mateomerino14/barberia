using Barberia.Domain.Common;

namespace Barberia.Domain.Staff;

/// <summary>Barbero del equipo: qué servicios hace, en qué horario trabaja y cuándo falta.</summary>
public sealed class Barber : Entity
{
    private readonly List<Guid> _serviceIds = [];
    private readonly List<DailyHours> _schedule = [];
    private readonly List<TimeOff> _timeOff = [];

    /// <summary>Constructor para la base de datos.</summary>
    private Barber()
    {
    }

    /// <summary>Nombre con el que lo conocen los clientes.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Indica si recibe turnos; uno inactivo deja de aparecer al reservar.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Servicios que hace.</summary>
    public IReadOnlyList<Guid> ServiceIds => _serviceIds;

    /// <summary>Horario semanal; un día sin horario no trabaja.</summary>
    public IReadOnlyList<DailyHours> Schedule => _schedule;

    /// <summary>Ausencias registradas.</summary>
    public IReadOnlyList<TimeOff> TimeOff => _timeOff;

    /// <summary>Crea un barbero activo sin servicios ni horario.</summary>
    public static Result<Barber> Create(string? name)
    {
        Barber barber = new();
        Result rename = barber.Rename(name);
        if (rename.IsFailure)
        {
            return rename.Error!;
        }

        return barber;
    }

    /// <summary>Cambia el nombre.</summary>
    public Result Rename(string? name)
    {
        Result<string> validName = Text.Required(name, Limits.NameMaxLength, StaffErrors.InvalidName);
        if (validName.IsFailure)
        {
            return validName;
        }

        Name = validName.Value;
        return Result.Success();
    }

    /// <summary>Reemplaza los servicios que hace.</summary>
    public void AssignServices(IEnumerable<Guid> serviceIds)
    {
        _serviceIds.Clear();
        _serviceIds.AddRange(serviceIds.Distinct());
    }

    /// <summary>Indica si hace un servicio.</summary>
    public bool Offers(Guid serviceId)
    {
        return _serviceIds.Contains(serviceId);
    }

    /// <summary>Reemplaza el horario semanal, que debe caber dentro del horario de la barbería.</summary>
    public Result SetSchedule(IReadOnlyCollection<DailyHours> schedule, IReadOnlyCollection<DailyHours> shopHours)
    {
        Result unique = DailyHours.EnsureOnePerDay(schedule);
        if (unique.IsFailure)
        {
            return unique;
        }

        bool fits = schedule.All(day => shopHours.Any(shopDay => shopDay.Covers(day)));
        if (!fits)
        {
            return StaffErrors.OutsideShopHours;
        }

        _schedule.Clear();
        _schedule.AddRange(schedule.OrderBy(item => item.Day).Select(item => item with { }));
        return Result.Success();
    }

    /// <summary>Registra una ausencia.</summary>
    public Result<TimeOff> AddTimeOff(DateOnly from, DateOnly to, string? reason)
    {
        Result<TimeOff> timeOff = Staff.TimeOff.Create(from, to, reason);
        if (timeOff.IsSuccess)
        {
            _timeOff.Add(timeOff.Value);
        }

        return timeOff;
    }

    /// <summary>Quita una ausencia.</summary>
    public Result RemoveTimeOff(Guid timeOffId)
    {
        int removed = _timeOff.RemoveAll(item => item.Id == timeOffId);
        if (removed == 0)
        {
            return StaffErrors.TimeOffNotFound;
        }

        return Result.Success();
    }

    /// <summary>Activa o desactiva al barbero.</summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    /// <summary>Devuelve el horario del barbero en una fecha, o nulo si ese día no trabaja.</summary>
    public DailyHours? HoursOn(DateOnly date)
    {
        if (_timeOff.Any(item => item.Includes(date)))
        {
            return null;
        }

        return _schedule.FirstOrDefault(hours => hours.Day == date.DayOfWeek);
    }
}
