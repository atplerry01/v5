namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public sealed class CanSuspendAvailabilitySpecification
{
    public bool IsSatisfiedBy(AvailabilityStatus status)
    {
        return status == AvailabilityStatus.Active;
    }
}
