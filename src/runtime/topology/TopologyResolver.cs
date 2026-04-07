using Whyce.Shared.Kernel.Determinism;

namespace Whyce.Runtime.Topology;

/// <summary>
/// Trusted-source topology resolver. Always reads from
/// <see cref="IStructureRegistry"/>; never trusts caller input.
/// </summary>
public sealed class TopologyResolver : ITopologyResolver
{
    private readonly IStructureRegistry _registry;

    public TopologyResolver(IStructureRegistry registry)
    {
        _registry = registry;
    }

    public TopologyCode Resolve(string spvId)
    {
        var node = _registry.Get(spvId);
        return new TopologyCode(node.ClusterCode, node.SubClusterCode, node.SpvCode);
    }
}
