namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public sealed class AvailabilitySpecification
{
    public bool IsSatisfiedBy(AvailabilityAggregate availability)
    {
        return availability.Id != default
            && availability.Range.EndTicks > availability.Range.StartTicks
            && Enum.IsDefined(availability.Status);
    }
}
