using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;

public sealed record ScheduleReconciliationRunCommand(
    Guid RunId,
    string Scope) : IHasAggregateId
{
    public Guid AggregateId => RunId;
}

public sealed record StartReconciliationRunCommand(
    Guid RunId,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => RunId;
}

public sealed record CompleteReconciliationRunCommand(
    Guid RunId,
    int ChecksProcessed,
    int DiscrepanciesFound,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => RunId;
}

public sealed record AbortReconciliationRunCommand(
    Guid RunId,
    string Reason,
    DateTimeOffset AbortedAt) : IHasAggregateId
{
    public Guid AggregateId => RunId;
}
