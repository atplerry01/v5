namespace Whycespace.Domain.StructuralSystem.Cluster.SubCluster;

public sealed class SubClusterException : Exception
{
    public SubClusterException(string message) : base(message) { }
}

public static class SubClusterErrors
{
    public static SubClusterException NotFound(Guid id)
        => new($"SubCluster '{id}' not found.");

    public static SubClusterException AlreadyActive(Guid id)
        => new($"SubCluster '{id}' is already active.");

    public static SubClusterException CannotDeactivateWithActiveSpvs(Guid id)
        => new($"SubCluster '{id}' cannot be deactivated while it has active SPVs.");
}
