namespace Whycespace.Projections.Orchestration.Workflow.Stage;

public sealed record StageView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
