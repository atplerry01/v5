using Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.ReconciliationRun;

namespace Whycespace.Projections.Control.SystemReconciliation.ReconciliationRun.Reducer;

public static class ReconciliationRunProjectionReducer
{
    public static ReconciliationRunReadModel Apply(ReconciliationRunReadModel state, ReconciliationRunScheduledEventSchema e) =>
        state with
        {
            RunId  = e.AggregateId,
            Scope  = e.Scope,
            Status = "Pending"
        };

    public static ReconciliationRunReadModel Apply(ReconciliationRunReadModel state, ReconciliationRunStartedEventSchema e) =>
        state with
        {
            Status    = "Running",
            StartedAt = e.StartedAt
        };

    public static ReconciliationRunReadModel Apply(ReconciliationRunReadModel state, ReconciliationRunCompletedEventSchema e) =>
        state with
        {
            Status              = "Completed",
            ChecksProcessed     = e.ChecksProcessed,
            DiscrepanciesFound  = e.DiscrepanciesFound,
            CompletedAt         = e.CompletedAt
        };

    public static ReconciliationRunReadModel Apply(ReconciliationRunReadModel state, ReconciliationRunAbortedEventSchema e) =>
        state with
        {
            Status      = "Aborted",
            AbortReason = e.Reason,
            AbortedAt   = e.AbortedAt
        };
}
