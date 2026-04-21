namespace Whycespace.Shared.Contracts.Events.Content.Document.Governance.Retention;

public sealed record RetentionAppliedEventSchema(
    Guid AggregateId,
    Guid TargetId,
    string TargetKind,
    DateTimeOffset WindowAppliedAt,
    DateTimeOffset WindowExpiresAt,
    string Reason,
    DateTimeOffset AppliedAt);

public sealed record RetentionHoldPlacedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset PlacedAt);

public sealed record RetentionReleasedEventSchema(
    Guid AggregateId,
    DateTimeOffset ReleasedAt);

public sealed record RetentionExpiredEventSchema(
    Guid AggregateId,
    DateTimeOffset ExpiredAt);

public sealed record RetentionMarkedEligibleForDestructionEventSchema(
    Guid AggregateId,
    DateTimeOffset MarkedAt);

public sealed record RetentionArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
