using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public readonly record struct ClusterId
{
    public Guid Value { get; }

    public ClusterId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ClusterId cannot be empty.");
        Value = value;
    }
}
