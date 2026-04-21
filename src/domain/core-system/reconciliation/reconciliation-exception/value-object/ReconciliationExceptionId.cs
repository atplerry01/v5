using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationException;

public readonly record struct ReconciliationExceptionId
{
    public Guid Value { get; }

    public ReconciliationExceptionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReconciliationExceptionId cannot be empty.");
        Value = value;
    }
}
