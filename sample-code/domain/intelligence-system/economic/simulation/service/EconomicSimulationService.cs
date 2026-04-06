namespace Whycespace.Domain.IntelligenceSystem.Economic.Simulation;

public sealed class EconomicSimulationService
{
    public SimulationResult Simulate(
        SimulationScenario scenario,
        decimal baseCost,
        decimal baseRevenue)
    {
        var simulatedCost = baseCost * scenario.CostMultiplier;
        var simulatedRevenue = baseRevenue * scenario.RevenueMultiplier;
        var profit = simulatedRevenue - simulatedCost;
        var variance = profit - (baseRevenue - baseCost);

        return new SimulationResult(
            profit,
            variance,
            $"Simulated with cost x{scenario.CostMultiplier}, revenue x{scenario.RevenueMultiplier}",
            profit >= 0 ? "PROFITABLE" : "LOSS",
            new ConfidenceScore(0.8m));
    }
}
