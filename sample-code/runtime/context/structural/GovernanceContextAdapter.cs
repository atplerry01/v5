using Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;
using Whycespace.Shared.Contracts.Domain.Structural;

namespace Whycespace.Runtime.Context.Structural;

/// <summary>
/// Adapts domain GovernanceExecutionContext to shared IGovernanceExecutionContext (E18.4).
/// Bridges domain-resolved governance context into the runtime pipeline
/// for WHYCEPOLICY evaluation.
/// </summary>
public sealed class GovernanceContextAdapter
{
    public IGovernanceExecutionContext Adapt(GovernanceExecutionContext context)
    {
        return new ResolvedGovernanceExecutionContext
        {
            RootEntityId = context.RootEntityId,
            ExecutionPath = context.ExecutionPath.ToList(),
            RelationshipTypes = context.RelationshipTypes.ToList(),
            GovernanceScope = context.GovernanceScope
        };
    }
}

public sealed record ResolvedGovernanceExecutionContext : IGovernanceExecutionContext
{
    public required Guid RootEntityId { get; init; }
    public required IReadOnlyCollection<Guid> ExecutionPath { get; init; }
    public required IReadOnlyCollection<string> RelationshipTypes { get; init; }
    public string? GovernanceScope { get; init; }
}
