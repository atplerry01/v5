using Whycespace.Engines.T1M.Domains.Economic.Transaction.State;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.Economic.Transaction.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Transaction.Steps;

/// <summary>
/// Step 1: Create and execute the transaction instruction — captures economic
/// intent (from → to, amount, currency, type).
/// </summary>
public sealed class ExecuteInstructionStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IEconomicMetrics _metrics;

    public ExecuteInstructionStep(
        ISystemIntentDispatcher dispatcher,
        IEconomicMetrics metrics)
    {
        _dispatcher = dispatcher;
        _metrics = metrics;
    }

    public string Name => TransactionLifecycleSteps.ExecuteInstruction;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute InstructionRoute = new("economic", "transaction", "instruction");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<TransactionLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("TransactionLifecycleWorkflowState not found in workflow context.");

        _metrics.RecordTransactionLifecycleStarted(state.Currency);

        var createCommand = new CreateInstructionCommand(
            state.InstructionId,
            state.FromAccountId,
            state.ToAccountId,
            state.Amount,
            state.Currency,
            state.InstructionType,
            state.InitiatedAt);

        var createResult = await _dispatcher.DispatchSystemAsync(createCommand, InstructionRoute, cancellationToken);
        if (!createResult.IsSuccess)
            return WorkflowStepResult.Failure(createResult.Error ?? "CreateInstruction dispatch failed.");

        var executeCommand = new ExecuteInstructionCommand(
            state.InstructionId,
            state.InitiatedAt);

        var executeResult = await _dispatcher.DispatchSystemAsync(executeCommand, InstructionRoute, cancellationToken);
        if (!executeResult.IsSuccess)
            return WorkflowStepResult.Failure(executeResult.Error ?? "ExecuteInstruction dispatch failed.");

        state.CurrentStep = TransactionLifecycleSteps.ExecuteInstruction;
        context.SetState(state);

        var events = new List<object>();
        events.AddRange(createResult.EmittedEvents);
        events.AddRange(executeResult.EmittedEvents);

        return WorkflowStepResult.Success(state.InstructionId, events);
    }
}
