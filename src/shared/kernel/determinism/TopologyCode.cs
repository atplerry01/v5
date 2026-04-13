namespace Whycespace.Shared.Kernel.Determinism;

/// <summary>
/// HSID v2.1 topology segment (CCC + XXX + SSSSSS = 12 chars).
/// Encodes Cluster (3) → SubCluster (3) → SPV (6).
/// </summary>
public sealed record TopologyCode(
    string Cluster,
    string SubCluster,
    string Spv)
{
    public override string ToString() => $"{Cluster}{SubCluster}{Spv}";
}
