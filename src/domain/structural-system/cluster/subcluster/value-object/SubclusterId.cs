using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public readonly record struct SubclusterId
{
    public Guid Value { get; }

    public SubclusterId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SubclusterId cannot be empty.");
        Value = value;
    }
}
