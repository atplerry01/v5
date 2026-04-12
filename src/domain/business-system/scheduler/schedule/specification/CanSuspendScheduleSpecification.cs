namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed class CanSuspendScheduleSpecification
{
    public bool IsSatisfiedBy(ScheduleStatus status)
    {
        return status == ScheduleStatus.Active;
    }
}
