namespace Whycespace.Projections.Operational.Incident;

public sealed record IncidentReadModel
{
    public required string IncidentId { get; init; }
    public required string IncidentType { get; init; }
    public required string Severity { get; init; }
    public required string Priority { get; init; }
    public required string Status { get; init; }
    public required string Source { get; init; }
    public string? AssignedTo { get; init; }
    public int EscalationLevel { get; init; }
    public string? ReferenceId { get; init; }
    public string SLAStatus { get; init; } = "within";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
}
