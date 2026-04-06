namespace Whycespace.Platform.Api.Core.Contracts.Simulation;

/// <summary>
/// Projected economic impact from a simulation run.
/// All values are estimates — no actual financial mutation occurs.
/// </summary>
public sealed record SimulationImpactView
{
    public decimal? EstimatedCost { get; init; }
    public decimal? EstimatedRevenue { get; init; }
    public decimal? ExposureRisk { get; init; }
}