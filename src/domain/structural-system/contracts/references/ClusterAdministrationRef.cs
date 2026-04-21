using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Contracts.References;

public readonly record struct ClusterAdministrationRef
{
    public Guid Value { get; }

    public ClusterAdministrationRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ClusterAdministrationRef cannot be empty.");
        Value = value;
    }
}
