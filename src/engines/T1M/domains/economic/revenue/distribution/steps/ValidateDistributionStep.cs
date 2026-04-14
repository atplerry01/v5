using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;

public sealed class ValidateDistributionStep : IWorkflowStep
{
    public string Name => DistributionWorkflowSteps.Validate;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Payload is not DistributionCreationIntent intent)
            return Task.FromResult(
                WorkflowStepResult.Failure("Payload is not a valid DistributionCreationIntent."));

        if (intent.DistributionId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("DistributionId is required."));

        if (string.IsNullOrWhiteSpace(intent.SpvId))
            return Task.FromResult(WorkflowStepResult.Failure("SpvId is required."));

        if (intent.TotalAmount <= 0m)
            return Task.FromResult(WorkflowStepResult.Failure("TotalAmount must be greater than zero."));

        if (intent.Allocations is null || intent.Allocations.Count == 0)
            return Task.FromResult(WorkflowStepResult.Failure("Distribution requires at least one allocation."));

        decimal percentageSum = 0m;
        foreach (var a in intent.Allocations)
        {
            if (string.IsNullOrWhiteSpace(a.ParticipantId))
                return Task.FromResult(WorkflowStepResult.Failure("Every allocation requires a ParticipantId."));
            if (a.OwnershipPercentage <= 0m || a.OwnershipPercentage > 100m)
                return Task.FromResult(WorkflowStepResult.Failure(
                    $"OwnershipPercentage must be in (0, 100] (was {a.OwnershipPercentage})."));
            percentageSum += a.OwnershipPercentage;
        }

        if (percentageSum != 100m)
            return Task.FromResult(WorkflowStepResult.Failure(
                $"Sum of allocation percentages must equal 100 (was {percentageSum})."));

        var state = new DistributionWorkflowState
        {
            DistributionId = intent.DistributionId,
            SpvId = intent.SpvId,
            TotalAmount = intent.TotalAmount,
            Allocations = intent.Allocations,
            CurrentStep = DistributionWorkflowSteps.Validate
        };
        context.SetState(state);

        return Task.FromResult(WorkflowStepResult.Success(intent));
    }
}
