using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public readonly record struct TopologyDescriptor
{
    public Guid ClusterReference { get; }
    public string TopologyName { get; }

    public TopologyDescriptor(Guid clusterReference, string topologyName)
    {
        Guard.Against(clusterReference == Guid.Empty, "TopologyDescriptor cluster reference cannot be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(topologyName), "TopologyDescriptor name must not be null or whitespace.");

        ClusterReference = clusterReference;
        TopologyName = topologyName;
    }
}
