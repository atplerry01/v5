using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationItem;

public sealed class ReconciliationItemAggregate : AggregateRoot
{
    public string SourceRef { get; private set; } = string.Empty;
    public string TargetRef { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public bool IsMatched { get; private set; }

    public static ReconciliationItemAggregate Create(Guid id, string sourceRef, string targetRef, decimal amount)
    {
        var agg = new ReconciliationItemAggregate
        {
            Id = id,
            SourceRef = sourceRef,
            TargetRef = targetRef,
            Amount = amount,
            IsMatched = false
        };
        agg.RaiseDomainEvent(new ReconciliationItemCreatedEvent(id, sourceRef, targetRef, amount));
        return agg;
    }

    public void Match()
    {
        IsMatched = true;
        RaiseDomainEvent(new ReconciliationItemMatchedEvent(Id));
    }
}
