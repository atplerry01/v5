namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public readonly record struct TopologyId
{
    public Guid Value { get; }

    public TopologyId(Guid value)
    {
        if (value == Guid.Empty)
            throw TopologyErrors.MissingId();

        Value = value;
    }
}
