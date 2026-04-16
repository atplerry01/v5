using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Discrepancy;

namespace Whycespace.Projections.Economic.Reconciliation.Discrepancy.Reducer;

public static class DiscrepancyProjectionReducer
{
    public static DiscrepancyReadModel Apply(DiscrepancyReadModel state, DiscrepancyDetectedEventSchema e) =>
        state with
        {
            DiscrepancyId    = e.DiscrepancyId,
            ProcessReference = e.ProcessReference,
            Source           = e.Source,
            ExpectedValue    = e.ExpectedValue,
            ActualValue      = e.ActualValue,
            Difference       = e.Difference,
            Status           = "Open",
            DetectedAt       = e.DetectedAt,
            LastUpdatedAt    = e.DetectedAt
        };

    public static DiscrepancyReadModel Apply(DiscrepancyReadModel state, DiscrepancyInvestigatedEventSchema e) =>
        state with
        {
            DiscrepancyId = e.DiscrepancyId,
            Status        = "Investigating"
        };

    public static DiscrepancyReadModel Apply(DiscrepancyReadModel state, DiscrepancyResolvedEventSchema e) =>
        state with
        {
            DiscrepancyId = e.DiscrepancyId,
            Resolution    = e.Resolution,
            Status        = "Resolved"
        };
}
