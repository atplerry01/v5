using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;

public sealed record CaptureObservabilityCommand(
    Guid ObservabilityId,
    Guid StreamId,
    Guid? ArchiveId,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    long Viewers,
    long Playbacks,
    long Errors,
    long Drops,
    long AverageBitrateBps,
    long AverageLatencyMs,
    DateTimeOffset CapturedAt) : IHasAggregateId
{
    public Guid AggregateId => ObservabilityId;
}

public sealed record UpdateObservabilityCommand(
    Guid ObservabilityId,
    long Viewers,
    long Playbacks,
    long Errors,
    long Drops,
    long AverageBitrateBps,
    long AverageLatencyMs,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => ObservabilityId;
}

public sealed record FinalizeObservabilityCommand(
    Guid ObservabilityId,
    DateTimeOffset FinalizedAt) : IHasAggregateId
{
    public Guid AggregateId => ObservabilityId;
}

public sealed record ArchiveObservabilityCommand(
    Guid ObservabilityId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => ObservabilityId;
}
