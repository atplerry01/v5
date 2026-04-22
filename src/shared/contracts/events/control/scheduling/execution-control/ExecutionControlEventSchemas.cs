namespace Whycespace.Shared.Contracts.Events.Control.Scheduling.ExecutionControl;

public sealed record ExecutionControlSignalIssuedEventSchema(
    Guid AggregateId,
    string JobInstanceId,
    string Signal,
    string ActorId,
    DateTimeOffset IssuedAt);

public sealed record ExecutionControlSignalOutcomeRecordedEventSchema(
    Guid AggregateId,
    string Outcome);
