namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record CompensationAction
{
    public SagaStepId StepId { get; }
    public string ActionType { get; }
    public DateTimeOffset ScheduledAt { get; }

    public CompensationAction(SagaStepId stepId, string actionType, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(stepId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);

        StepId = stepId;
        ActionType = actionType;
        ScheduledAt = timestamp;
    }

    public override string ToString() => $"{ActionType} for step {StepId}";
}
