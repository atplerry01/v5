namespace Whycespace.Domain.BusinessSystem.Scheduler.Calendar;

public sealed record WorkingDays(IReadOnlySet<DayOfWeek> Days)
{
    public static readonly WorkingDays Default = new(
        new HashSet<DayOfWeek>
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        });

    public bool IsWorkingDay(DayOfWeek day) => Days.Contains(day);
}
