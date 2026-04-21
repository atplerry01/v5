using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Contracts.References;

public readonly record struct ClusterAuthorityRef
{
    public Guid Value { get; }

    public ClusterAuthorityRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ClusterAuthorityRef cannot be empty.");
        Value = value;
    }
}
