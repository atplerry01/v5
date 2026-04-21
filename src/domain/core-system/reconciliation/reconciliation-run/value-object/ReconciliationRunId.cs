using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationRun;

public readonly record struct ReconciliationRunId
{
    public Guid Value { get; }

    public ReconciliationRunId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReconciliationRunId cannot be empty.");
        Value = value;
    }
}
