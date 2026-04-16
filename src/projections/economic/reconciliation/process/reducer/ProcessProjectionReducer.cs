using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Process;

namespace Whycespace.Projections.Economic.Reconciliation.Process.Reducer;

public static class ProcessProjectionReducer
{
    public static ProcessReadModel Apply(ProcessReadModel state, ReconciliationTriggeredEventSchema e) =>
        state with
        {
            ProcessId         = e.ProcessId,
            LedgerReference   = e.LedgerReference,
            ObservedReference = e.ObservedReference,
            Status            = "Pending",
            TriggeredAt       = e.TriggeredAt,
            LastUpdatedAt     = e.TriggeredAt
        };

    public static ProcessReadModel Apply(ProcessReadModel state, ReconciliationMatchedEventSchema e) =>
        state with
        {
            ProcessId = e.ProcessId,
            Status    = "Matched"
        };

    public static ProcessReadModel Apply(ProcessReadModel state, ReconciliationMismatchedEventSchema e) =>
        state with
        {
            ProcessId = e.ProcessId,
            Status    = "Mismatched"
        };

    public static ProcessReadModel Apply(ProcessReadModel state, ReconciliationResolvedEventSchema e) =>
        state with
        {
            ProcessId = e.ProcessId,
            Status    = "Resolved"
        };
}
