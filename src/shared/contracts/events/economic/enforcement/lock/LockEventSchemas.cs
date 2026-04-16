namespace Whycespace.Shared.Contracts.Events.Economic.Enforcement.Lock;

public sealed record SystemLockedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string Scope,
    string Reason,
    DateTimeOffset LockedAt);

public sealed record SystemUnlockedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string UnlockReason,
    DateTimeOffset UnlockedAt);
