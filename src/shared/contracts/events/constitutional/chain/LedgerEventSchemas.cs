namespace Whycespace.Shared.Contracts.Events.Constitutional.Chain;

public sealed record LedgerOpenedEventSchema(
    Guid AggregateId,
    string LedgerName,
    DateTimeOffset OpenedAt);

public sealed record LedgerSealedEventSchema(
    Guid AggregateId,
    DateTimeOffset SealedAt);
