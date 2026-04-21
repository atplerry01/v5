using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Contracts.References;

public readonly record struct ClusterRef
{
    public Guid Value { get; }

    public ClusterRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ClusterRef cannot be empty.");
        Value = value;
    }
}
