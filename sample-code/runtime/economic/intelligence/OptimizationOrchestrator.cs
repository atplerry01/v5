using Whycespace.Domain.IntelligenceSystem.Economic.Optimization;
using Whycespace.Runtime.Economic.Intelligence.Result;

namespace Whycespace.Runtime.Economic.Intelligence;

/// <summary>
/// Economic execution path optimization orchestrator (E17.8).
/// Evaluates candidate execution paths for profit maximization
/// and cost efficiency scoring.
///
/// CRITICAL: Read-only intelligence — no state mutation.
/// Runtime applies decisions via the execution pipeline (E17.5-E17.7).
/// </summary>
public sealed class OptimizationOrchestrator
{
    private readonly EconomicOptimizationService _optimizer;

    public OptimizationOrchestrator(EconomicOptimizationService optimizer)
    {
        _optimizer = optimizer;
    }

    public OptimizationExecutionResult Execute(IEnumerable<ExecutionOption> options)
    {
        var result = _optimizer.Optimize(options);

        var efficiencies = result.AllOptions
            .ToDictionary(
                x => x,
                x => _optimizer.CalculateEfficiency(x));

        return OptimizationExecutionResult.Success(
            result.BestOption,
            efficiencies);
    }
}
