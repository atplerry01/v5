namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class GovernanceExecutionContext
{
    public Guid RootEntityId { get; }
    public IReadOnlyCollection<Guid> ExecutionPath { get; }
    public IReadOnlyCollection<string> RelationshipTypes { get; }
    public string? GovernanceScope { get; }

    public GovernanceExecutionContext(
        Guid rootEntityId,
        IEnumerable<Guid> executionPath,
        IEnumerable<string> relationshipTypes,
        string? governanceScope)
    {
        RootEntityId = rootEntityId;
        ExecutionPath = executionPath.ToList().AsReadOnly();
        RelationshipTypes = relationshipTypes.Distinct().ToList().AsReadOnly();
        GovernanceScope = governanceScope;
    }
}
