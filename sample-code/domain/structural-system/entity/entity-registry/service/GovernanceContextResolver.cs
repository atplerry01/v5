namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class GovernanceContextResolver
{
    public GovernanceExecutionContext Resolve(
        Guid rootEntityId,
        EntityExecutionPath path,
        IEnumerable<EntityRelationship> relationships,
        string? governanceScope)
    {
        var relationshipTypes = relationships
            .Where(r => path.Contains(r.FromEntityId) &&
                        path.Contains(r.ToEntityId))
            .Select(r => r.RelationshipType)
            .OrderBy(x => x);

        return new GovernanceExecutionContext(
            rootEntityId,
            path.Nodes,
            relationshipTypes,
            governanceScope);
    }
}
