namespace Whycespace.Engines.T2E.Business.Scheduler.Availability;

public record AvailabilityCommand(
    string Action,
    string EntityId,
    object Payload
);
