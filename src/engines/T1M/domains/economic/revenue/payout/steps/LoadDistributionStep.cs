using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// Step 1: retrieves the distribution coordinates (SpvId, participant shares)
/// from the intent payload and hydrates the typed PayoutWorkflowState.
/// In this phase the intent already carries the pre-resolved vault ids;
/// a future phase may replace this with a projection query.
/// </summary>
public sealed class LoadDistributionStep : IWorkflowStep
{
    public string Name => PayoutExecutionSteps.LoadDistribution;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Payload is not PayoutExecutionIntent intent)
            return Task.FromResult(
                WorkflowStepResult.Failure("Payload is not a valid PayoutExecutionIntent."));

        if (intent.PayoutId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("PayoutId is required."));

        if (intent.DistributionId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("DistributionId is required."));

        if (intent.ContractId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("ContractId is required."));

        if (string.IsNullOrWhiteSpace(intent.SpvId))
            return Task.FromResult(WorkflowStepResult.Failure("SpvId is required."));

        if (intent.SpvVaultId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("SpvVaultId is required."));

        if (intent.Shares is null || intent.Shares.Count == 0)
            return Task.FromResult(WorkflowStepResult.Failure("Payout requires at least one participant share."));

        foreach (var s in intent.Shares)
        {
            if (string.IsNullOrWhiteSpace(s.ParticipantId))
                return Task.FromResult(WorkflowStepResult.Failure("Every share requires a ParticipantId."));
            if (s.ParticipantVaultId == Guid.Empty)
                return Task.FromResult(WorkflowStepResult.Failure("Every share requires a ParticipantVaultId."));
            if (s.Amount <= 0m)
                return Task.FromResult(WorkflowStepResult.Failure("Share amount must be greater than zero."));
        }

        var state = new PayoutWorkflowState
        {
            PayoutId = intent.PayoutId,
            DistributionId = intent.DistributionId,
            ContractId = intent.ContractId,
            IdempotencyKey = $"payout|{intent.DistributionId:N}|{intent.SpvId}",
            SpvId = intent.SpvId,
            SpvVaultId = intent.SpvVaultId,
            Shares = intent.Shares,
            CurrentStep = PayoutExecutionSteps.LoadDistribution
        };
        context.SetState(state);

        return Task.FromResult(WorkflowStepResult.Success(intent));
    }
}
