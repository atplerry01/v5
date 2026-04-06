namespace Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

public sealed class EconomicOptimizationService
{
    public OptimizationResult Optimize(IEnumerable<ExecutionOption> options)
    {
        var list = options.ToList();

        var best = list
            .OrderByDescending(x => x.Profit)
            .First();

        return new OptimizationResult(best, list);
    }

    public decimal CalculateEfficiency(ExecutionOption option)
    {
        if (option.EstimatedCost == 0)
            return 0;

        return option.Profit / option.EstimatedCost;
    }
}
