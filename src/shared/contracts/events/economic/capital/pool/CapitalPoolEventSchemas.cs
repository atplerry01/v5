namespace Whycespace.Shared.Contracts.Events.Economic.Capital.Pool;

public sealed record PoolCreatedEventSchema(
    Guid AggregateId,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record CapitalAggregatedEventSchema(
    Guid AggregateId,
    Guid SourceAccountId,
    decimal Amount,
    decimal NewPoolTotal);

public sealed record CapitalReducedEventSchema(
    Guid AggregateId,
    Guid SourceAccountId,
    decimal Amount,
    decimal NewPoolTotal);
