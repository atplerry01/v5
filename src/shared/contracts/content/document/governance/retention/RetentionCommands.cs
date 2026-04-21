using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.Governance.Retention;

public sealed record ApplyRetentionCommand(
    Guid RetentionId,
    Guid TargetId,
    string TargetKind,
    DateTimeOffset WindowAppliedAt,
    DateTimeOffset WindowExpiresAt,
    string Reason,
    DateTimeOffset AppliedAt) : IHasAggregateId
{
    public Guid AggregateId => RetentionId;
}

public sealed record PlaceRetentionHoldCommand(
    Guid RetentionId,
    string Reason,
    DateTimeOffset PlacedAt) : IHasAggregateId
{
    public Guid AggregateId => RetentionId;
}

public sealed record ReleaseRetentionCommand(
    Guid RetentionId,
    DateTimeOffset ReleasedAt) : IHasAggregateId
{
    public Guid AggregateId => RetentionId;
}

public sealed record ExpireRetentionCommand(
    Guid RetentionId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => RetentionId;
}

public sealed record MarkRetentionEligibleForDestructionCommand(
    Guid RetentionId,
    DateTimeOffset MarkedAt) : IHasAggregateId
{
    public Guid AggregateId => RetentionId;
}

public sealed record ArchiveRetentionCommand(
    Guid RetentionId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => RetentionId;
}
