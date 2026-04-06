namespace Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

public sealed class ExecutionOption
{
    public IReadOnlyCollection<Guid> Path { get; }
    public decimal EstimatedCost { get; }
    public decimal EstimatedRevenue { get; }

    public ExecutionOption(
        IEnumerable<Guid> path,
        decimal cost,
        decimal revenue)
    {
        Path = path.Distinct().ToList().AsReadOnly();
        EstimatedCost = cost;
        EstimatedRevenue = revenue;
    }

    public decimal Profit => EstimatedRevenue - EstimatedCost;
}
