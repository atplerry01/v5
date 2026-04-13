namespace Whycespace.Runtime.Topology;

/// <summary>
/// One node in the constitutional structure registry. Encodes the trust
/// triple (Cluster, SubCluster, SPV) for a single SPV id.
/// </summary>
public sealed record StructureNode(
    string SpvId,
    string ClusterCode,
    string SubClusterCode,
    string SpvCode);
