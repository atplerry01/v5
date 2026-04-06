using Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;
using Whycespace.Shared.Contracts.Domain.Structural;

namespace Whycespace.Runtime.Context.Structural;

/// <summary>
/// Resolves IEntityGraphContext from the structural relationship graph (E18.3).
/// Provides execution path intelligence for multi-entity workflows,
/// economic routing, and governance overlay evaluation.
/// </summary>
public sealed class EntityGraphContextResolver
{
    private readonly EntityGraphPathResolver _resolver;

    public EntityGraphContextResolver(EntityGraphPathResolver resolver)
    {
        _resolver = resolver;
    }

    public IEntityGraphContext Resolve(
        Guid start,
        Guid end,
        IEnumerable<EntityRelationship> relationships)
    {
        var path = _resolver.ResolveExecutionPath(start, end, relationships);

        return new ResolvedEntityGraphContext
        {
            StartEntityId = start,
            TargetEntityId = end,
            ExecutionPath = path.Nodes.ToList()
        };
    }
}

public sealed record ResolvedEntityGraphContext : IEntityGraphContext
{
    public required Guid StartEntityId { get; init; }
    public required Guid TargetEntityId { get; init; }
    public required IReadOnlyCollection<Guid> ExecutionPath { get; init; }
}
