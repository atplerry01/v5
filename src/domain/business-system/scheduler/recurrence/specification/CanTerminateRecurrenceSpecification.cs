namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed class CanTerminateRecurrenceSpecification
{
    public bool IsSatisfiedBy(RecurrenceStatus status)
    {
        return status == RecurrenceStatus.Active;
    }
}
