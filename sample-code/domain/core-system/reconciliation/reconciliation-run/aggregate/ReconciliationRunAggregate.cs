using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationRun;

public sealed class ReconciliationRunAggregate : AggregateRoot
{
    public string RunType { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }

    public static ReconciliationRunAggregate Create(Guid id, string runType, string scope)
    {
        var agg = new ReconciliationRunAggregate
        {
            Id = id,
            RunType = runType,
            Scope = scope,
            IsCompleted = false
        };
        agg.RaiseDomainEvent(new ReconciliationRunCreatedEvent(id, runType, scope));
        return agg;
    }

    public void Complete()
    {
        IsCompleted = true;
        RaiseDomainEvent(new ReconciliationRunCompletedEvent(Id));
    }
}
