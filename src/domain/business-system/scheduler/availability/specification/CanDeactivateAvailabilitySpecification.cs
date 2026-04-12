namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public sealed class CanDeactivateAvailabilitySpecification
{
    public bool IsSatisfiedBy(AvailabilityStatus status)
    {
        return status == AvailabilityStatus.Active || status == AvailabilityStatus.Suspended;
    }
}
