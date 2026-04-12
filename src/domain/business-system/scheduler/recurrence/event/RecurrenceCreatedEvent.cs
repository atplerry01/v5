namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed record RecurrenceCreatedEvent(RecurrenceId RecurrenceId, RecurrencePattern Pattern);
