using Whycespace.Engines.T1M.Wss.Dispatcher;
using Whycespace.Engines.T1M.Wss.Registry;

namespace Whycespace.Engines.T1M.Wss.Execution;

/// <summary>
/// Stateless step executor — resolves step from registry and invokes via dispatcher.
/// MUST NOT contain domain logic or workflow definitions.
/// </summary>
public sealed class StepExecutor
{
    private readonly StepRegistry _registry;
    private readonly EngineInvocationDispatcher _dispatcher;

    public StepExecutor(StepRegistry registry, EngineInvocationDispatcher dispatcher)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task<StepExecutionResult> ExecuteAsync(
        string stepName,
        WorkflowExecutionContext context,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        ArgumentNullException.ThrowIfNull(context);

        if (!_registry.HasStep(stepName))
            return new StepExecutionResult(stepName, false, $"Step '{stepName}' not registered.");

        var engineTarget = _registry.ResolveEngine(stepName);

        var dispatched = await _dispatcher.DispatchAsync(
            engineTarget,
            stepName,
            context,
            ct);

        return new StepExecutionResult(stepName, dispatched, dispatched ? null : $"Dispatch failed for step '{stepName}'.");
    }
}

public sealed record StepExecutionResult(
    string StepName,
    bool Success,
    string? FailureReason = null);
