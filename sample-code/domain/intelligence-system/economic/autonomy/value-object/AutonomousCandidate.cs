namespace Whycespace.Domain.IntelligenceSystem.Economic.Autonomy;

public sealed class AutonomousCandidate
{
    public IReadOnlyCollection<Guid> Path { get; }
    public decimal EstimatedCost { get; }
    public decimal EstimatedRevenue { get; }

    public AutonomousCandidate(
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
