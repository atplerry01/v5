namespace Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilityCapturedEventSchema(
    Guid AggregateId,
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
    DateTimeOffset CapturedAt);

public sealed record ObservabilityUpdatedEventSchema(
    Guid AggregateId,
    long PreviousViewers,
    long PreviousPlaybacks,
    long PreviousErrors,
    long PreviousDrops,
    long PreviousAverageBitrateBps,
    long PreviousAverageLatencyMs,
    long NewViewers,
    long NewPlaybacks,
    long NewErrors,
    long NewDrops,
    long NewAverageBitrateBps,
    long NewAverageLatencyMs,
    DateTimeOffset UpdatedAt);

public sealed record ObservabilityFinalizedEventSchema(
    Guid AggregateId,
    DateTimeOffset FinalizedAt);

public sealed record ObservabilityArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
