using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public readonly record struct SourceReference
{
    public Guid Value { get; }

    public SourceReference(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SourceReference cannot be empty.");
        Value = value;
    }
}
