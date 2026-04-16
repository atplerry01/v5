namespace Whycespace.Shared.Contracts.Events.Economic.Routing.Execution;

public sealed record ExecutionStartedEventSchema(
    Guid AggregateId,
    Guid PathId,
    DateTimeOffset StartedAt);

public sealed record ExecutionCompletedEventSchema(
    Guid AggregateId,
    DateTimeOffset CompletedAt);

public sealed record ExecutionFailedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);

public sealed record ExecutionAbortedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset AbortedAt);
