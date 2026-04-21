using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Observability;

namespace Whycespace.Projections.Content.Streaming.DeliveryGovernance.Observability.Reducer;

public static class ObservabilityProjectionReducer
{
    public static ObservabilityReadModel Apply(ObservabilityReadModel state, ObservabilityCapturedEventSchema e) =>
        state with
        {
            ObservabilityId = e.AggregateId,
            StreamId = e.StreamId,
            ArchiveId = e.ArchiveId,
            WindowStart = e.WindowStart,
            WindowEnd = e.WindowEnd,
            Viewers = e.Viewers,
            Playbacks = e.Playbacks,
            Errors = e.Errors,
            Drops = e.Drops,
            AverageBitrateBps = e.AverageBitrateBps,
            AverageLatencyMs = e.AverageLatencyMs,
            Status = "Capturing",
            CapturedAt = e.CapturedAt,
            LastModifiedAt = e.CapturedAt
        };

    public static ObservabilityReadModel Apply(ObservabilityReadModel state, ObservabilityUpdatedEventSchema e) =>
        state with
        {
            ObservabilityId = e.AggregateId,
            Viewers = e.NewViewers,
            Playbacks = e.NewPlaybacks,
            Errors = e.NewErrors,
            Drops = e.NewDrops,
            AverageBitrateBps = e.NewAverageBitrateBps,
            AverageLatencyMs = e.NewAverageLatencyMs,
            Status = "Updated",
            LastModifiedAt = e.UpdatedAt
        };

    public static ObservabilityReadModel Apply(ObservabilityReadModel state, ObservabilityFinalizedEventSchema e) =>
        state with
        {
            ObservabilityId = e.AggregateId,
            Status = "Finalized",
            FinalizedAt = e.FinalizedAt,
            LastModifiedAt = e.FinalizedAt
        };

    public static ObservabilityReadModel Apply(ObservabilityReadModel state, ObservabilityArchivedEventSchema e) =>
        state with
        {
            ObservabilityId = e.AggregateId,
            Status = "Archived",
            LastModifiedAt = e.ArchivedAt
        };
}
