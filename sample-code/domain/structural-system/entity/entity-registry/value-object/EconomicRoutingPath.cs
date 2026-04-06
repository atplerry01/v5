namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EconomicRoutingPath
{
    public Guid SourceEntityId { get; }
    public Guid TargetEntityId { get; }
    public IReadOnlyCollection<Guid> Path { get; }

    public EconomicRoutingPath(Guid source, Guid target, IEnumerable<Guid> path)
    {
        SourceEntityId = source;
        TargetEntityId = target;
        Path = path.ToList().AsReadOnly();
    }
}
