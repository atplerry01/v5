namespace Whycespace.Projections.Orchestration.Workflow.Execution;

public sealed record ExecutionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
