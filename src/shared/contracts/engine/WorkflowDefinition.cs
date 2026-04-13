namespace Whycespace.Shared.Contracts.Engine;

public sealed record WorkflowDefinition
{
    public required string Name { get; init; }
    public required IReadOnlyList<WorkflowStepDefinition> Steps { get; init; }
}
