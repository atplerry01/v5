namespace Whycespace.Shared.Contracts.Domain.Structural;

/// <summary>
/// Graph-aware execution context for multi-entity orchestration (E18.3).
/// Resolved by the runtime from the structural relationship graph.
/// Provides execution path intelligence to WSS, economic engines, and policy evaluation.
/// </summary>
public interface IEntityGraphContext
{
    Guid StartEntityId { get; }
    Guid TargetEntityId { get; }
    IReadOnlyCollection<Guid> ExecutionPath { get; }
}
