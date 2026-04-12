namespace Whycespace.Domain.BusinessSystem.Scheduler.Calendar;

public sealed class CalendarSpecification
{
    public bool IsSatisfiedBy(CalendarAggregate calendar)
    {
        return calendar.Id != default
            && Enum.IsDefined(calendar.Status);
    }
}
