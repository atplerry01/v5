using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public readonly record struct ProcessReference
{
    public Guid Value { get; }

    public ProcessReference(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProcessReference cannot be empty.");
        Value = value;
    }
}
