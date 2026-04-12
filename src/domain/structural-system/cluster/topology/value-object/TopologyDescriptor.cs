namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public readonly record struct TopologyDescriptor
{
    public Guid ClusterReference { get; }
    public string TopologyName { get; }

    public TopologyDescriptor(Guid clusterReference, string topologyName)
    {
        if (clusterReference == Guid.Empty)
            throw TopologyErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(topologyName))
            throw TopologyErrors.MissingDescriptor();

        ClusterReference = clusterReference;
        TopologyName = topologyName;
    }
}
