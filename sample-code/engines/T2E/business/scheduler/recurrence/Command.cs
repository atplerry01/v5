namespace Whycespace.Engines.T2E.Business.Scheduler.Recurrence;

public record RecurrenceCommand(
    string Action,
    string EntityId,
    object Payload
);
