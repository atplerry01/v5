using Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;
using Whycespace.Runtime.Economic.Result;

namespace Whycespace.Runtime.Economic;

/// <summary>
/// Graph-execution reconciliation orchestrator (E17.7).
/// Compares expected (graph-routed) vs actual (ledger/settlement) state
/// and resolves recovery instructions when anomalies are detected.
///
/// CRITICAL: Domain computes results — runtime executes recovery.
/// </summary>
public sealed class ReconciliationOrchestrator
{
    private readonly ReconciliationEngine _engine;
    private readonly RecoveryResolver _resolver;

    public ReconciliationOrchestrator(
        ReconciliationEngine engine,
        RecoveryResolver resolver)
    {
        _engine = engine;
        _resolver = resolver;
    }

    public ReconciliationExecutionResult Execute(
        ExpectedExecutionSnapshot expected,
        ActualExecutionSnapshot actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        var result = _engine.Evaluate(expected, actual);

        if (result.IsHealthy())
            return ReconciliationExecutionResult.Healthy();

        var recovery = _resolver.Resolve(result);

        return ReconciliationExecutionResult.Unhealthy(recovery);
    }
}
