namespace Whycespace.Projections.Orchestration.Workflow.Escalation;

public sealed record EscalationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
