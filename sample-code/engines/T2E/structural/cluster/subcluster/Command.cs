namespace Whycespace.Engines.T2E.Structural.Cluster.Subcluster;

public record SubclusterCommand(string Action, string EntityId, object Payload);

public sealed record CreateSubClusterCommand(string Id, string AuthorityId, string Name)
    : SubclusterCommand("Create", Id, null!);

public sealed record ActivateSubClusterCommand(string Id)
    : SubclusterCommand("Activate", Id, null!);

public sealed record DeactivateSubClusterCommand(string Id, string Reason)
    : SubclusterCommand("Deactivate", Id, null!);

public sealed record AddSubClusterSpvCommand(string SubClusterId, string SpvId)
    : SubclusterCommand("AddSpv", SubClusterId, null!);

public sealed record RemoveSubClusterSpvCommand(string SubClusterId, string SpvId)
    : SubclusterCommand("RemoveSpv", SubClusterId, null!);
