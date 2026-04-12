namespace Whycespace.Domain.BusinessSystem.Scheduler.Calendar;

public sealed class CanArchiveCalendarSpecification
{
    public bool IsSatisfiedBy(CalendarStatus status)
    {
        return status == CalendarStatus.Active;
    }
}
