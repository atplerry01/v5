namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityBinding
{
    public Guid EntityId { get; }
    public Guid? EconomicAccountId { get; }
    public Guid? WorkflowContextId { get; }
    public string? GovernanceScope { get; }
    public Guid? IdentityId { get; }

    public EntityBinding(
        Guid entityId,
        Guid? economicAccountId,
        Guid? workflowContextId,
        string? governanceScope,
        Guid? identityId)
    {
        EntityId = entityId;
        EconomicAccountId = economicAccountId;
        WorkflowContextId = workflowContextId;
        GovernanceScope = governanceScope;
        IdentityId = identityId;
    }
}
