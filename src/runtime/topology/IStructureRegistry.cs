namespace Whycespace.Runtime.Topology;

/// <summary>
/// Authoritative source of constitutional structure for HSID v2.1 topology
/// resolution. Looked up by SPV id; never accepts caller-supplied topology.
///
/// Guard reference: deterministic-id.guard.md G14.
/// </summary>
public interface IStructureRegistry
{
    StructureNode Get(string spvId);
    bool Contains(string spvId);
}
