using Whycespace.Domain.EconomicSystem.Routing.Execution;
using Whycespace.Shared.Contracts.Domain.Economic;

namespace Whycespace.Runtime.Economic;

/// <summary>
/// Graph-aware economic execution orchestrator (E17.5).
/// Resolves routing instructions from the structural graph execution path
/// and coordinates multi-entity economic flows via T2E engines.
///
/// CRITICAL: Orchestrates execution — does NOT contain business logic.
/// Economic rules remain in domain services.
/// </summary>
public sealed class GraphEconomicExecutionOrchestrator
{
    private readonly EconomicFlowRouter _router;

    public GraphEconomicExecutionOrchestrator(EconomicFlowRouter router)
    {
        _router = router;
    }

    public GraphEconomicExecutionResult Execute(IEconomicGraphContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ExecutionPath == null || context.ExecutionPath.Count == 0)
            return GraphEconomicExecutionResult.Failed("Invalid execution path");

        var executionPath = new EconomicExecutionPath(
            context.SourceEntityId,
            context.TargetEntityId,
            context.ExecutionPath);

        var flows = _router.Route(executionPath, context.Amount, context.Currency).ToList();

        if (flows.Count == 0)
            return GraphEconomicExecutionResult.Failed("No routing instructions generated");

        return GraphEconomicExecutionResult.Routed(flows.Count);
    }
}

public sealed record GraphEconomicExecutionResult
{
    public required bool IsSuccess { get; init; }
    public int FlowCount { get; init; }
    public string? ErrorMessage { get; init; }

    public static GraphEconomicExecutionResult Routed(int flowCount) =>
        new() { IsSuccess = true, FlowCount = flowCount };

    public static GraphEconomicExecutionResult Failed(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}
