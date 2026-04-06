namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed record ClusterStatus(string Value)
{
    public static readonly ClusterStatus Created = new("created");
    public static readonly ClusterStatus Active = new("active");
    public static readonly ClusterStatus SystemCritical = new("system_critical");

    public bool IsTerminal => this == SystemCritical;
}
