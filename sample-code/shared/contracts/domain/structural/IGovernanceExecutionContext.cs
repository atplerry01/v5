namespace Whycespace.Shared.Contracts.Domain.Structural;

/// <summary>
/// Governance execution context for graph-aware policy evaluation (E18.4).
/// Resolved from the structural graph by the runtime.
/// Provides execution path + relationship types + governance scope
/// for WHYCEPOLICY pre-execution validation.
/// </summary>
public interface IGovernanceExecutionContext
{
    Guid RootEntityId { get; }
    IReadOnlyCollection<Guid> ExecutionPath { get; }
    IReadOnlyCollection<string> RelationshipTypes { get; }
    string? GovernanceScope { get; }
}
