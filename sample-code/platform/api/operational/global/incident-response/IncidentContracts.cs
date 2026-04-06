namespace Whycespace.Platform.Api.Operational.Global.IncidentResponse;

public sealed record CreateIncidentDto { public string? IncidentId { get; init; } public required string Type { get; init; } public required string Severity { get; init; } public string? Source { get; init; } public string? Description { get; init; } }
public sealed record AssignIncidentDto { public required string AggregateId { get; init; } public required string AssigneeIdentityId { get; init; } public int EscalationLevel { get; init; } = 1; }
public sealed record ResolveIncidentDto { public required string AggregateId { get; init; } }
public sealed record CloseIncidentDto { public required string AggregateId { get; init; } }
public sealed record IncidentQueryResponse { public required string IncidentId { get; init; } public required string IncidentType { get; init; } public required string Severity { get; init; } public required string Status { get; init; } public string? AssignedTo { get; init; } }
