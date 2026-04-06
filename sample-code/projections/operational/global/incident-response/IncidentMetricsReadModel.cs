namespace Whycespace.Projections.Operational.Incident;

public sealed record IncidentMetricsReadModel
{
    public int TotalIncidents { get; init; }
    public int OpenIncidents { get; init; }
    public int ResolvedIncidents { get; init; }
    public int ClosedIncidents { get; init; }
    public int EscalatedIncidents { get; init; }
    public double AverageResolutionTimeMinutes { get; init; }
    public int SLABreachCount { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
