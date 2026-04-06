namespace Whycespace.Domain.IntelligenceSystem.Economic.Simulation;

public sealed class SimulationScenario
{
    public IReadOnlyCollection<Guid> Path { get; }
    public decimal CostMultiplier { get; }
    public decimal RevenueMultiplier { get; }

    public SimulationScenario(
        IEnumerable<Guid> path,
        decimal costMultiplier,
        decimal revenueMultiplier)
    {
        Path = path.Distinct().ToList().AsReadOnly();
        CostMultiplier = costMultiplier;
        RevenueMultiplier = revenueMultiplier;
    }
}
