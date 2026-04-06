namespace Whycespace.Projections.Orchestration.Workflow.Assignment;

public sealed record AssignmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
