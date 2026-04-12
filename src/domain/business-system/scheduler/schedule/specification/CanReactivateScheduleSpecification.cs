namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed class CanReactivateScheduleSpecification
{
    public bool IsSatisfiedBy(ScheduleStatus status)
    {
        return status == ScheduleStatus.Suspended;
    }
}
