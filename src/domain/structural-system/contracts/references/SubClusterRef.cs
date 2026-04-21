using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Contracts.References;

public readonly record struct SubClusterRef
{
    public Guid Value { get; }

    public SubClusterRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SubClusterRef cannot be empty.");
        Value = value;
    }
}
