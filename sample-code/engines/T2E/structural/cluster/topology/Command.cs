namespace Whycespace.Engines.T2E.Structural.Cluster.Topology;

public record TopologyCommand(string Action, string EntityId, object Payload);
public sealed record CreateTopologyCommand(string Id, string AuthorityId, string Name) : TopologyCommand("Create", Id, null!);
public sealed record AddTopologySpvCommand(string SubClusterId, string SpvId) : TopologyCommand("AddSpv", SubClusterId, null!);
