using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.DiscrepancyResolution;

namespace Whycespace.Projections.Control.SystemReconciliation.DiscrepancyResolution.Reducer;

public static class DiscrepancyResolutionProjectionReducer
{
    public static DiscrepancyResolutionReadModel Apply(DiscrepancyResolutionReadModel state, DiscrepancyResolutionInitiatedEventSchema e) =>
        state with
        {
            ResolutionId = e.AggregateId,
            DetectionId  = e.DetectionId,
            Status       = "Initiated",
            InitiatedAt  = e.InitiatedAt
        };

    public static DiscrepancyResolutionReadModel Apply(DiscrepancyResolutionReadModel state, DiscrepancyResolutionCompletedEventSchema e) =>
        state with
        {
            Status      = "Completed",
            Outcome     = e.Outcome,
            Notes       = e.Notes,
            CompletedAt = e.CompletedAt
        };
}
