namespace Whycespace.Engines.T2E.Structural.Cluster.Authority;

public record AuthorityCommand(string Action, string EntityId, object Payload);
public sealed record CreateAuthorityCommand(string Id, string ClusterId, string Name) : AuthorityCommand("Create", Id, null!);
public sealed record AddAuthoritySubClusterCommand(string AuthorityId, string SubClusterId) : AuthorityCommand("AddSubCluster", AuthorityId, null!);
