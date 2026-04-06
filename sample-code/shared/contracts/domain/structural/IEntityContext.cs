namespace Whycespace.Shared.Contracts.Domain.Structural;

/// <summary>
/// Structural entity context for cross-system binding (E18.1).
/// Resolved from the EntityAggregate by the runtime.
/// Carries entity identity and system bindings through the execution pipeline.
/// </summary>
public interface IEntityContext
{
    Guid EntityId { get; }
    Guid? EconomicAccountId { get; }
    Guid? WorkflowContextId { get; }
    string? GovernanceScope { get; }
    Guid? IdentityId { get; }
}
