namespace Whycespace.Shared.Contracts.Events.Control.Scheduling.SystemJob;

public sealed record SystemJobDefinedEventSchema(
    Guid AggregateId,
    string Name,
    string Category,
    TimeSpan Timeout);

public sealed record SystemJobDeprecatedEventSchema(
    Guid AggregateId);
