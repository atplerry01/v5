using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public readonly record struct TargetId
{
    public Guid Value { get; }

    public TargetId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TargetId cannot be empty.");
        Value = value;
    }
}
