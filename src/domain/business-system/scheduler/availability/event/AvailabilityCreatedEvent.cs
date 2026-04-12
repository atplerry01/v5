namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public sealed record AvailabilityCreatedEvent(AvailabilityId AvailabilityId, TimeRange Range);
