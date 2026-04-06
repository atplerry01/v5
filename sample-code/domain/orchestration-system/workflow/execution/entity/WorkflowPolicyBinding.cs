namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

using Whycespace.Domain.SharedKernel;

public sealed class WorkflowPolicyBinding : Entity
{
    public Guid StepId { get; private set; }
    public PolicyCheckpoint Checkpoint { get; private set; } = default!;
    public TransitionPolicy RequiredPolicyRule { get; private set; } = default!;

    private WorkflowPolicyBinding() { }

    public static WorkflowPolicyBinding Create(
        Guid bindingId,
        Guid stepId,
        PolicyCheckpoint checkpoint,
        TransitionPolicy requiredPolicyRule)
    {
        if (bindingId == Guid.Empty)
            throw new ArgumentException("BindingId cannot be empty.", nameof(bindingId));
        if (stepId == Guid.Empty)
            throw new ArgumentException("StepId cannot be empty.", nameof(stepId));
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentNullException.ThrowIfNull(requiredPolicyRule);

        return new WorkflowPolicyBinding
        {
            Id = bindingId,
            StepId = stepId,
            Checkpoint = checkpoint,
            RequiredPolicyRule = requiredPolicyRule
        };
    }

    public bool IsBlocking => Checkpoint.IsBlocking;
}
