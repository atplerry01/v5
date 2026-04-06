namespace Whycespace.Engines.T2E.Operational.Incident;

public sealed record IncidentCreatedEventDto
{
    public required string AggregateId { get; init; }
    public required string IncidentType { get; init; }
    public required string Severity { get; init; }
    public required string Priority { get; init; }
    public required string Source { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public sealed record IncidentAssignedEventDto
{
    public required string AggregateId { get; init; }
    public required Guid AssigneeIdentityId { get; init; }
    public required int EscalationLevel { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public sealed record IncidentEscalatedEventDto
{
    public required string AggregateId { get; init; }
    public required string PreviousSeverity { get; init; }
    public required string NewSeverity { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public sealed record IncidentResolvedEventDto
{
    public required string AggregateId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public sealed record IncidentClosedEventDto
{
    public required string AggregateId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
