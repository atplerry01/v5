namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public static class ClusterErrors
{
    public static InvalidOperationException MissingId() =>
        new("ClusterId cannot be empty.");

    public static InvalidOperationException MissingDescriptor() =>
        new("ClusterDescriptor requires non-empty ClusterName and ClusterType.");

    public static InvalidOperationException InvalidStateTransition(ClusterStatus status, string action) =>
        new($"Cannot {action} a cluster in {status} status.");
}
