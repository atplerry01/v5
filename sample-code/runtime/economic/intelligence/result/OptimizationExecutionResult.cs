using Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

namespace Whycespace.Runtime.Economic.Intelligence.Result;

public sealed record OptimizationExecutionResult
{
    public required ExecutionOption BestOption { get; init; }
    public required IReadOnlyDictionary<ExecutionOption, decimal> EfficiencyScores { get; init; }

    public static OptimizationExecutionResult Success(
        ExecutionOption best,
        IReadOnlyDictionary<ExecutionOption, decimal> scores) =>
        new() { BestOption = best, EfficiencyScores = scores };
}
