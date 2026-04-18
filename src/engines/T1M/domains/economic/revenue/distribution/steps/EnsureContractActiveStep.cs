using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;

public sealed class EnsureContractActiveStep : IWorkflowStep
{
    private readonly IContractStatusGate _gate;

    public EnsureContractActiveStep(IContractStatusGate gate) => _gate = gate;

    public string Name => DistributionWorkflowSteps.EnsureContractActive;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Payload is not DistributionCreationIntent intent)
            return WorkflowStepResult.Failure("Payload is not a valid DistributionCreationIntent.");

        if (intent.ContractId == Guid.Empty)
            return WorkflowStepResult.Failure("ContractId is required for contract gating.");

        var result = await _gate.CheckAsync(intent.ContractId, cancellationToken);
        if (!result.IsActive)
            return WorkflowStepResult.Failure(result.Reason);

        return WorkflowStepResult.Success(intent);
    }
}
