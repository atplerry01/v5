namespace Whycespace.Projections.Orchestration.Workflow.Definition;

public sealed record DefinitionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
