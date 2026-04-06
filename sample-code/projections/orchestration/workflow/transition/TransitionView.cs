namespace Whycespace.Projections.Orchestration.Workflow.Transition;

public sealed record TransitionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
