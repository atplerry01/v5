namespace Whycespace.Domain.StructuralSystem.Cluster.SubCluster;

public sealed record SubClusterStatus(string Value)
{
    public static readonly SubClusterStatus Created = new("Created");
    public static readonly SubClusterStatus Active = new("Active");
    public static readonly SubClusterStatus Deactivated = new("Deactivated");

    public bool IsTerminal => this == Deactivated;

    public override string ToString() => Value;
}
