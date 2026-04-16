namespace Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;

public sealed record EscalationReadModel
{
    public Guid SubjectId { get; init; }
    public int ViolationCount { get; init; }
    public int SeverityScore { get; init; }
    public string EscalationLevel { get; init; } = "None";
    public DateTimeOffset WindowStart { get; init; }
    public DateTimeOffset LastViolationAt { get; init; }
    public Guid LastViolationId { get; init; }
    public int Version { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
