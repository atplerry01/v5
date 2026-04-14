namespace Whycespace.Shared.Contracts.Events.Economic.Capital.Reserve;

public sealed record ReserveCreatedEventSchema(
    Guid AggregateId,
    Guid AccountId,
    decimal ReservedAmount,
    string Currency,
    DateTimeOffset ReservedAt,
    DateTimeOffset ExpiresAt);

public sealed record ReserveReleasedEventSchema(
    Guid AggregateId,
    Guid AccountId,
    decimal Amount,
    DateTimeOffset ReleasedAt);

public sealed record ReserveExpiredEventSchema(
    Guid AggregateId,
    Guid AccountId,
    decimal Amount,
    DateTimeOffset ExpiredAt);
