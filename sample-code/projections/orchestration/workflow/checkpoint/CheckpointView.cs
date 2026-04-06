namespace Whycespace.Projections.Orchestration.Workflow.Checkpoint;

public sealed record CheckpointView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
