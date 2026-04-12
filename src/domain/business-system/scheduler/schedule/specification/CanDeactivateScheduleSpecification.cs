namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed class CanDeactivateScheduleSpecification
{
    public bool IsSatisfiedBy(ScheduleStatus status)
    {
        return status == ScheduleStatus.Active || status == ScheduleStatus.Suspended;
    }
}
