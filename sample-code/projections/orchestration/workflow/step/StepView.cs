namespace Whycespace.Projections.Orchestration.Workflow.Step;

public sealed record StepView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
