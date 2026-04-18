using Whycespace.Engines.T1M.Domains.Economic.Transaction.State;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Economic.Transaction.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Transaction.Steps;

/// <summary>
/// Step 2: Initiate the transaction envelope — wraps the instruction into
/// a transaction with settlement + ledger references for downstream tracing.
/// </summary>
public sealed class InitiateTransactionStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public InitiateTransactionStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => TransactionLifecycleSteps.InitiateTransaction;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute TransactionRoute = new("economic", "transaction", "transaction");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<TransactionLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("TransactionLifecycleWorkflowState not found in workflow context.");

        var references = new List<TransactionReferenceDto>
        {
            new("instruction", state.InstructionId),
            new("settlement", state.SettlementId),
            new("journal", state.JournalId)
        };

        var command = new InitiateTransactionCommand(
            state.TransactionId,
            state.InstructionType,
            references,
            state.InitiatedAt);

        var result = await _dispatcher.DispatchSystemAsync(command, TransactionRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "InitiateTransaction dispatch failed.");

        state.CurrentStep = TransactionLifecycleSteps.InitiateTransaction;
        context.SetState(state);

        return WorkflowStepResult.Success(state.TransactionId, result.EmittedEvents);
    }
}
