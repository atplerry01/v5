namespace Whycespace.Projections.Simulation;

/// <summary>
/// Projected simulation result for analytics and audit.
/// Key = "simulation:{simulationId}".
/// </summary>
public sealed record SimulationResultReadModel
{
    public required string SimulationId { get; init; }
    public required string ScenarioType { get; init; }
    public required string SubjectId { get; init; }
    public required string Action { get; init; }
    public required string Resource { get; init; }
    public required bool PolicyAllowed { get; init; }
    public required string PredictedDecision { get; init; }
    public double RiskScore { get; init; }
    public string? RiskCategory { get; init; }
    public string? RecommendationSummary { get; init; }
    public int RecommendationCount { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? WorkflowId { get; init; }
    public required DateTimeOffset SimulatedAt { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public long LastEventVersion { get; init; }

    public static string KeyFor(string simulationId) => $"simulation:{simulationId}";
}
