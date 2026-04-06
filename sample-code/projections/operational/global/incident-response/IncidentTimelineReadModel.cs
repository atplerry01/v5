namespace Whycespace.Projections.Operational.Incident;

public sealed record IncidentTimelineReadModel
{
    public required string IncidentId { get; init; }
    public required IReadOnlyList<TimelineEntry> Entries { get; init; }
}

public sealed record TimelineEntry
{
    public required DateTimeOffset Timestamp { get; init; }
    public required string Action { get; init; }
    public string? Actor { get; init; }
    public string? Metadata { get; init; }
}
