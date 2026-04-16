using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Reconciliation.Process;

public sealed record TriggerReconciliationCommand(
    Guid ProcessId,
    Guid LedgerReference,
    Guid ObservedReference,
    DateTimeOffset TriggeredAt) : IHasAggregateId
{
    public Guid AggregateId => ProcessId;
}

public sealed record MarkMatchedCommand(Guid ProcessId) : IHasAggregateId
{
    public Guid AggregateId => ProcessId;
}

public sealed record MarkMismatchedCommand(Guid ProcessId) : IHasAggregateId
{
    public Guid AggregateId => ProcessId;
}

public sealed record ResolveReconciliationCommand(Guid ProcessId) : IHasAggregateId
{
    public Guid AggregateId => ProcessId;
}
