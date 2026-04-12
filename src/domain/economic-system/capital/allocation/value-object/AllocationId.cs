using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public readonly record struct AllocationId
{
    public Guid Value { get; }

    public AllocationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AllocationId cannot be empty.");
        Value = value;
    }
}
