namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Graph-aware economic execution context (E17.5).
/// Resolved from the structural relationship graph by the runtime.
/// Provides execution path + amount + currency for graph-driven economic flows.
/// </summary>
public interface IEconomicGraphContext
{
    Guid SourceEntityId { get; }
    Guid TargetEntityId { get; }
    IReadOnlyCollection<Guid> ExecutionPath { get; }
    decimal Amount { get; }
    string Currency { get; }
}
