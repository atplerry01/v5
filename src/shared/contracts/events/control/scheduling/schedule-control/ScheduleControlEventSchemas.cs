namespace Whycespace.Shared.Contracts.Events.Control.Scheduling.ScheduleControl;

public sealed record ScheduleControlDefinedEventSchema(
    Guid AggregateId,
    string JobDefinitionId,
    string TriggerExpression);

public sealed record ScheduleControlSuspendedEventSchema(
    Guid AggregateId);

public sealed record ScheduleControlResumedEventSchema(
    Guid AggregateId);

public sealed record ScheduleControlRetiredEventSchema(
    Guid AggregateId);
