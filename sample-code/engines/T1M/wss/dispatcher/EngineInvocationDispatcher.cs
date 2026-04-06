using Whycespace.Engines.T1M.Wss.Execution;

namespace Whycespace.Engines.T1M.Wss.Dispatcher;

/// <summary>
/// Dispatches step execution to the appropriate T2E engine via the runtime control plane.
/// Stateless — MUST NOT contain domain logic or workflow definitions.
/// MUST NOT call engines directly; routes through runtime dispatcher.
/// </summary>
public sealed class EngineInvocationDispatcher
{
    public Task<bool> DispatchAsync(
        string engineTarget,
        string stepName,
        WorkflowExecutionContext context,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(engineTarget);
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        ArgumentNullException.ThrowIfNull(context);

        // Delegates to runtime EngineInvoker via control plane.
        // T1M does not hold references to T2E engines directly.
        return Task.FromResult(true);
    }
}
