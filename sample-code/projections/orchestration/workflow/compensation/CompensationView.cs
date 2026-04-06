namespace Whycespace.Projections.Orchestration.Workflow.Compensation;

public sealed record CompensationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
