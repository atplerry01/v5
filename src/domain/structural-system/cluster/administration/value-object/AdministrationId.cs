using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public readonly record struct AdministrationId
{
    public Guid Value { get; }

    public AdministrationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AdministrationId cannot be empty.");
        Value = value;
    }
}
