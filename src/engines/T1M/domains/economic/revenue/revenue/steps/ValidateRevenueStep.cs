using Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.Steps;

public sealed class ValidateRevenueStep : IWorkflowStep
{
    public string Name => RevenueProcessingSteps.Validate;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Payload is not RevenueProcessingIntent intent)
            return Task.FromResult(
                WorkflowStepResult.Failure("Payload is not a valid RevenueProcessingIntent."));

        if (intent.RevenueId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("RevenueId is required."));

        if (intent.ContractId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("ContractId is required."));

        if (string.IsNullOrWhiteSpace(intent.SpvId))
            return Task.FromResult(WorkflowStepResult.Failure("SpvId is required."));

        if (intent.Amount <= 0m)
            return Task.FromResult(WorkflowStepResult.Failure("Amount must be greater than zero."));

        if (intent.VaultAccountId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("VaultAccountId is required."));

        if (string.IsNullOrWhiteSpace(intent.Currency))
            return Task.FromResult(WorkflowStepResult.Failure("Currency is required."));

        var state = new RevenueWorkflowState
        {
            RevenueId = intent.RevenueId,
            ContractId = intent.ContractId,
            SpvId = intent.SpvId,
            VaultAccountId = intent.VaultAccountId,
            Amount = intent.Amount,
            Currency = intent.Currency,
            SourceRef = intent.SourceRef,
            CurrentStep = RevenueProcessingSteps.Validate
        };
        context.SetState(state);

        return Task.FromResult(WorkflowStepResult.Success(intent));
    }
}
