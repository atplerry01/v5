namespace Whyce.Shared.Contracts.Engine;

public sealed record WorkflowStepDefinition
{
    public required string StepId { get; init; }
    public required string StepName { get; init; }
    public required WorkflowStepType StepType { get; init; }
    public required Type StepHandlerType { get; init; }
}
