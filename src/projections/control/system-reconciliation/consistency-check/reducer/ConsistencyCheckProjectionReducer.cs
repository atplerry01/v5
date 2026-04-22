using Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.ConsistencyCheck;

namespace Whycespace.Projections.Control.SystemReconciliation.ConsistencyCheck.Reducer;

public static class ConsistencyCheckProjectionReducer
{
    public static ConsistencyCheckReadModel Apply(ConsistencyCheckReadModel state, ConsistencyCheckInitiatedEventSchema e) =>
        state with
        {
            CheckId     = e.AggregateId,
            ScopeTarget = e.ScopeTarget,
            Status      = "Initiated",
            InitiatedAt = e.InitiatedAt
        };

    public static ConsistencyCheckReadModel Apply(ConsistencyCheckReadModel state, ConsistencyCheckCompletedEventSchema e) =>
        state with
        {
            Status           = "Completed",
            HasDiscrepancies = e.HasDiscrepancies,
            CompletedAt      = e.CompletedAt
        };
}
