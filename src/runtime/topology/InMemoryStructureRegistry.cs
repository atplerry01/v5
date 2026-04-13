using System.Collections.Concurrent;

namespace Whycespace.Runtime.Topology;

/// <summary>
/// In-memory <see cref="IStructureRegistry"/> stub. Seeded at composition
/// time from configuration; intended as a placeholder until the canonical
/// constitutional registry is wired in. Not a production data source.
/// </summary>
public sealed class InMemoryStructureRegistry : IStructureRegistry
{
    private readonly ConcurrentDictionary<string, StructureNode> _nodes;

    public InMemoryStructureRegistry(IEnumerable<StructureNode> seed)
    {
        _nodes = new ConcurrentDictionary<string, StructureNode>(
            seed.ToDictionary(n => n.SpvId, n => n));
    }

    public StructureNode Get(string spvId) =>
        _nodes.TryGetValue(spvId, out var node)
            ? node
            : throw new InvalidOperationException(
                $"HSID topology resolution failed: SPV '{spvId}' not registered.");

    public bool Contains(string spvId) => _nodes.ContainsKey(spvId);
}
