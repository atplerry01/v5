using Whyce.Shared.Kernel.Determinism;

namespace Whyce.Runtime.Topology;

/// <summary>
/// HSID v2.1 topology resolver. Maps an SPV id to its authoritative
/// <see cref="TopologyCode"/>. Topology is NEVER taken from a request body
/// or caller-supplied field — only from this resolver.
///
/// Guard reference: deterministic-id.guard.md G14.
/// </summary>
public interface ITopologyResolver
{
    TopologyCode Resolve(string spvId);
}
