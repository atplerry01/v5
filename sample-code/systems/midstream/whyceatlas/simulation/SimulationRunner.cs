using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Midstream.WhyceAtlas.Simulation;

/// <summary>
/// Simulation flow:
/// 1. Create isolated sandbox (SimulationSandbox implements ISystemIntentDispatcher)
/// 2. Execute workflow against sandbox — captures all intents without real side effects
/// 3. Evaluate the execution trace (SimulationEvaluator)
/// 4. Return verdict: PASS / RISK / FAIL
/// </summary>
public sealed class SimulationRunner
{
    private readonly SimulationEvaluator _evaluator;
    private readonly IClock _clock;

    public SimulationRunner(SimulationEvaluator evaluator, IClock clock)
    {
        _evaluator = evaluator;
        _clock = clock;
    }

    public async Task<SimulationResult> RunAsync(
        Func<ISystemIntentDispatcher, CancellationToken, Task<IntentResult>> workflowAction,
        CancellationToken cancellationToken = default)
    {
        var sandbox = new SimulationSandbox(_clock);

        IntentResult executionResult;
        try
        {
            executionResult = await workflowAction(sandbox, cancellationToken);
        }
        catch (Exception ex)
        {
            return new SimulationResult
            {
                Success = false,
                Evaluation = null,
                ExecutionResult = IntentResult.Fail(Guid.Empty, $"Simulation threw exception: {ex.Message}", "SIMULATION_EXCEPTION"),
                Error = ex.Message
            };
        }

        var evaluation = _evaluator.Evaluate(sandbox);

        return new SimulationResult
        {
            Success = evaluation.Verdict == SimulationVerdict.Pass,
            Evaluation = evaluation,
            ExecutionResult = executionResult,
            Error = evaluation.Verdict == SimulationVerdict.Fail ? evaluation.Reason : null
        };
    }
}

public sealed record SimulationResult
{
    public required bool Success { get; init; }
    public SimulationEvaluation? Evaluation { get; init; }
    public required IntentResult ExecutionResult { get; init; }
    public string? Error { get; init; }
}
