using Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.Steps;

/// <summary>
/// T3.6 — gates the revenue workflow on Contract.Status == Active. Reads
/// ContractId from the validated intent (workflow state may not yet be
/// hydrated when this runs as the first step).
/// </summary>
public sealed class EnsureContractActiveStep : IWorkflowStep
{
    private readonly IContractStatusGate _gate;

    public EnsureContractActiveStep(IContractStatusGate gate) => _gate = gate;

    public string Name => RevenueProcessingSteps.EnsureContractActive;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Payload is not RevenueProcessingIntent intent)
            return WorkflowStepResult.Failure("Payload is not a valid RevenueProcessingIntent.");

        if (intent.ContractId == Guid.Empty)
            return WorkflowStepResult.Failure("ContractId is required for contract gating.");

        var result = await _gate.CheckAsync(intent.ContractId, cancellationToken);
        if (!result.IsActive)
            return WorkflowStepResult.Failure(result.Reason);

        return WorkflowStepResult.Success(intent);
    }
}
