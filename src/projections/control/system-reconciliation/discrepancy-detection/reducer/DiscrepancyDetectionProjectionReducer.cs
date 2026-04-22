using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.DiscrepancyDetection;

namespace Whycespace.Projections.Control.SystemReconciliation.DiscrepancyDetection.Reducer;

public static class DiscrepancyDetectionProjectionReducer
{
    public static DiscrepancyDetectionReadModel Apply(DiscrepancyDetectionReadModel state, DiscrepancyDetectedEventSchema e) =>
        state with
        {
            DetectionId     = e.AggregateId,
            Kind            = e.Kind,
            SourceReference = e.SourceReference,
            Status          = "Detected",
            DetectedAt      = e.DetectedAt
        };

    public static DiscrepancyDetectionReadModel Apply(DiscrepancyDetectionReadModel state, DiscrepancyDetectionDismissedEventSchema e) =>
        state with
        {
            Status          = "Dismissed",
            DismissalReason = e.Reason
        };
}
