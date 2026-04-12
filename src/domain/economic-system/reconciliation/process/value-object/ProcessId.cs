using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public readonly record struct ProcessId
{
    public Guid Value { get; }

    public ProcessId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProcessId cannot be empty.");
        Value = value;
    }
}
