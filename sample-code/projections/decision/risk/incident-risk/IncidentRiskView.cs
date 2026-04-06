namespace Whycespace.Projections.Decision.Risk.IncidentRisk;

public sealed record IncidentRiskView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
