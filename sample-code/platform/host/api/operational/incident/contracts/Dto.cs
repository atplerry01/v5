namespace Whycespace.Platform.Api.Operational.Incident.Contracts;

public sealed record CreateIncidentRequest
{
    public string? CommandId { get; init; }
    public string? IncidentId { get; init; }
    public required string Type { get; init; }
    public required string Severity { get; init; }
    public string? ReferenceId { get; init; }
    public string? Source { get; init; }
    public string? Description { get; init; }
    public string? ReferenceDomain { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
}
