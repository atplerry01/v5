using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// Phase 7 T7.4 — posts an append-only compensating journal that reverses
/// the original payout journal. No existing entry is mutated or deleted;
/// the reversal is a fresh balanced journal whose net effect cancels the
/// original (debits↔credits swapped). Journal + entry ids are derived
/// deterministically from PayoutId so retries converge on the same row.
/// </summary>
public sealed class PostCompensatingLedgerJournalStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _ids;

    public PostCompensatingLedgerJournalStep(ISystemIntentDispatcher dispatcher, IIdGenerator ids)
    {
        _dispatcher = dispatcher;
        _ids = ids;
    }

    public string Name => PayoutCompensationSteps.PostCompensatingLedgerJournal;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute LedgerRoute = new("economic", "ledger", "journal");
    private const string PayoutCurrency = "USD";

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<PayoutCompensationWorkflowState>()
            ?? throw new InvalidOperationException(
                "PayoutCompensationWorkflowState not found in workflow context.");

        if (state.OriginalJournalId == Guid.Empty)
            return WorkflowStepResult.Failure(
                "OriginalJournalId must be set before posting a compensating journal.");

        var ledgerId = _ids.Generate($"ledger|{state.SpvId}");
        var compensatingJournalId = _ids.Generate($"journal|compensating|{state.PayoutId:N}");

        var entries = new List<JournalEntryInput>(state.Shares.Count * 2);
        foreach (var share in state.Shares)
        {
            // Sign reversal: original Debit becomes Credit, original Credit becomes Debit.
            // Deterministic entry ids are derived from the compensating journal id so
            // replay is idempotent and entries remain distinct from the original journal.
            var reverseCreditId = _ids.Generate(
                $"comp-credit|{state.PayoutId:N}|{share.ParticipantId}");
            entries.Add(new JournalEntryInput(
                reverseCreditId,
                state.SpvVaultId,
                share.Amount,
                PayoutCurrency,
                "Credit"));

            var reverseDebitId = _ids.Generate(
                $"comp-debit|{state.PayoutId:N}|{share.ParticipantId}");
            entries.Add(new JournalEntryInput(
                reverseDebitId,
                share.ParticipantVaultId,
                share.Amount,
                PayoutCurrency,
                "Debit"));
        }

        var command = new PostCompensatingJournalCommand(
            ledgerId,
            compensatingJournalId,
            entries,
            new CompensationReference(state.OriginalJournalId, state.Reason));

        var result = await _dispatcher.DispatchSystemAsync(command, LedgerRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(
                result.Error ?? "PostCompensatingLedgerJournal dispatch failed.");

        state.CompensatingJournalId = compensatingJournalId;
        state.CurrentStep = PayoutCompensationSteps.PostCompensatingLedgerJournal;
        context.SetState(state);

        return WorkflowStepResult.Success(compensatingJournalId, result.EmittedEvents);
    }
}
