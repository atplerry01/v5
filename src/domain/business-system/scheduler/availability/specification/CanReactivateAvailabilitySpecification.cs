namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public sealed class CanReactivateAvailabilitySpecification
{
    public bool IsSatisfiedBy(AvailabilityStatus status)
    {
        return status == AvailabilityStatus.Suspended;
    }
}
