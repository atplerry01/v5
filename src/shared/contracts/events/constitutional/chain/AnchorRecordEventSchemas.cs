namespace Whycespace.Shared.Contracts.Events.Constitutional.Chain;

public sealed record AnchorRecordCreatedEventSchema(
    Guid AggregateId,
    Guid CorrelationId,
    string BlockHash,
    string EventHash,
    string PreviousBlockHash,
    string DecisionHash,
    long Sequence,
    DateTimeOffset AnchoredAt);

public sealed record AnchorRecordSealedEventSchema(
    Guid AggregateId,
    DateTimeOffset SealedAt);
