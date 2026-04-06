namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityExecutionPath
{
    private readonly List<Guid> _nodes;

    public IReadOnlyCollection<Guid> Nodes => _nodes.AsReadOnly();

    public EntityExecutionPath(IEnumerable<Guid> nodes)
    {
        _nodes = nodes.Distinct().ToList();
    }

    public bool Contains(Guid entityId) => _nodes.Contains(entityId);
}
