using Barberia.Domain.Common;

namespace Barberia.Application.Features;

/// <summary>Horario de un día tal como llega desde afuera.</summary>
public sealed record HoursInput(DayOfWeek Day, TimeOnly Opens, TimeOnly Closes);

/// <summary>Convierte horarios de entrada en horarios del dominio.</summary>
public static class HoursInputExtensions
{
    /// <summary>Crea los horarios del dominio, o devuelve el primer error.</summary>
    public static Result<IReadOnlyList<DailyHours>> ToDailyHours(this IEnumerable<HoursInput> inputs)
    {
        List<DailyHours> hours = [];
        foreach (HoursInput input in inputs)
        {
            Result<DailyHours> day = DailyHours.Create(input.Day, input.Opens, input.Closes);
            if (day.IsFailure)
            {
                return day.Error!;
            }

            hours.Add(day.Value);
        }

        return hours;
    }
}
