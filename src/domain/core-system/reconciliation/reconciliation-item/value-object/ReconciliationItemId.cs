using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationItem;

public readonly record struct ReconciliationItemId
{
    public Guid Value { get; }

    public ReconciliationItemId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReconciliationItemId cannot be empty.");
        Value = value;
    }
}
