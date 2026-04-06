using Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;
using Whycespace.Shared.Contracts.Domain.Structural;

namespace Whycespace.Runtime.Context.Structural;

/// <summary>
/// Resolves IEntityContext from an EntityAggregate's binding state (E18.1).
/// Provides cross-system binding context for economic, workflow, governance,
/// and identity pipeline stages.
/// </summary>
public sealed class EntityContextResolver
{
    public IEntityContext Resolve(EntityAggregate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ResolvedEntityContext
        {
            EntityId = entity.Id,
            EconomicAccountId = entity.Binding?.EconomicAccountId,
            WorkflowContextId = entity.Binding?.WorkflowContextId,
            GovernanceScope = entity.Binding?.GovernanceScope,
            IdentityId = entity.Binding?.IdentityId
        };
    }
}

public sealed record ResolvedEntityContext : IEntityContext
{
    public required Guid EntityId { get; init; }
    public Guid? EconomicAccountId { get; init; }
    public Guid? WorkflowContextId { get; init; }
    public string? GovernanceScope { get; init; }
    public Guid? IdentityId { get; init; }
}
