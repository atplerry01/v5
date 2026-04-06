namespace Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

public sealed class OptimizationResult
{
    public ExecutionOption BestOption { get; }
    public IReadOnlyCollection<ExecutionOption> AllOptions { get; }

    public OptimizationResult(
        ExecutionOption best,
        IEnumerable<ExecutionOption> all)
    {
        BestOption = best;
        AllOptions = all.ToList().AsReadOnly();
    }
}
