namespace Whycespace.Projections.Orchestration.Workflow.Instance;

public sealed record InstanceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
