using Whycespace.Engines.T1M.Domains.Economic.Transaction.State;
using Whycespace.Shared.Contracts.Economic.Transaction.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Transaction.Steps;

/// <summary>
/// Phase 4 — entry step for the transaction lifecycle workflow. Hydrates
/// <see cref="TransactionLifecycleWorkflowState"/> from the inbound
/// <see cref="TransactionLifecycleIntent"/> so every downstream step can
/// rely on <c>context.GetState&lt;TransactionLifecycleWorkflowState&gt;()</c>
/// returning a populated instance.
///
/// Matches the validate-then-hydrate convention used by the revenue,
/// distribution, and payout workflows. Required-field checks fail fast so
/// the control plane never advances on a malformed intent.
/// </summary>
public sealed class ValidateLifecycleIntentStep : IWorkflowStep
{
    public string Name => "validate_lifecycle_intent";
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Payload is not TransactionLifecycleIntent intent)
            return Task.FromResult(
                WorkflowStepResult.Failure("Payload is not a valid TransactionLifecycleIntent."));

        if (intent.TransactionId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("TransactionId is required."));
        if (intent.InstructionId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("InstructionId is required."));
        if (intent.SettlementId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("SettlementId is required."));
        if (intent.LedgerId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("LedgerId is required."));
        if (intent.JournalId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("JournalId is required."));
        if (intent.FromAccountId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("FromAccountId is required."));
        if (intent.ToAccountId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("ToAccountId is required."));
        if (intent.Amount <= 0m)
            return Task.FromResult(WorkflowStepResult.Failure("Amount must be greater than zero."));
        if (string.IsNullOrWhiteSpace(intent.Currency))
            return Task.FromResult(WorkflowStepResult.Failure("Currency is required."));

        var state = new TransactionLifecycleWorkflowState
        {
            InstructionId       = intent.InstructionId,
            TransactionId       = intent.TransactionId,
            SettlementId        = intent.SettlementId,
            LedgerId            = intent.LedgerId,
            JournalId           = intent.JournalId,
            FromAccountId       = intent.FromAccountId,
            ToAccountId         = intent.ToAccountId,
            Amount              = intent.Amount,
            Currency            = intent.Currency,
            InstructionType     = intent.InstructionType,
            SettlementProvider  = intent.SettlementProvider,
            InitiatedAt         = intent.InitiatedAt,
            FxBaseCurrency      = intent.FxBaseCurrency,
            CurrentStep         = "validate_lifecycle_intent"
        };
        context.SetState(state);

        return Task.FromResult(WorkflowStepResult.Success(intent));
    }
}
