namespace Whycespace.Projections.Operational.Global.IncidentResponse;

public sealed record IncidentResponseView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
