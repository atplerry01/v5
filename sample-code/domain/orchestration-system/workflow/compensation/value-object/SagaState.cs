namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public enum SagaState
{
    Pending,
    Running,
    Completed,
    Failed,
    Compensating
}
