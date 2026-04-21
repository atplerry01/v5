using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public readonly record struct TopologyId
{
    public Guid Value { get; }

    public TopologyId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TopologyId cannot be empty.");
        Value = value;
    }
}
