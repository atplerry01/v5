namespace Whycespace.Projections.Orchestration.Workflow.Queue;

public sealed record QueueView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
