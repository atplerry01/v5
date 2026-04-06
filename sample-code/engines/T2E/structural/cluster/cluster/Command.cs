namespace Whycespace.Engines.T2E.Structural.Cluster.Cluster;

public record ClusterCommand(string Action, string EntityId, object Payload);
public sealed record CreateClusterCommand(string Id, string Name, string Jurisdiction) : ClusterCommand("Create", Id, null!);
public sealed record ActivateClusterCommand(string Id) : ClusterCommand("Activate", Id, null!);
public sealed record AddClusterAuthorityCommand(string ClusterId, string AuthorityId) : ClusterCommand("AddAuthority", ClusterId, null!);
