using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Contracts.References;

public readonly record struct ClusterProviderRef
{
    public Guid Value { get; }

    public ClusterProviderRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ClusterProviderRef cannot be empty.");
        Value = value;
    }
}
