namespace Whycespace.Shared.Contracts.Events.Control.Observability.SystemSignal;

public sealed record SystemSignalDefinedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind,
    string Source);

public sealed record SystemSignalDeprecatedEventSchema(
    Guid AggregateId);
