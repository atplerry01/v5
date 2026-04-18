using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// T3.5 — explicit ledger emission after a successful payout. Builds a
/// balanced journal (one Debit per SPV slice + one Credit per participant
/// share) and dispatches PostJournalEntriesCommand to the ledger BC. Both
/// LedgerId, JournalId, and EntryIds are deterministic functions of the
/// PayoutId so retries converge on the same journal row and the
/// LedgerJournalPostedEvent is emitted at most once per payout.
/// </summary>
public sealed class PostLedgerJournalStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _ids;

    public PostLedgerJournalStep(ISystemIntentDispatcher dispatcher, IIdGenerator ids)
    {
        _dispatcher = dispatcher;
        _ids = ids;
    }

    public string Name => PayoutExecutionSteps.PostLedgerJournal;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute LedgerRoute = new("economic", "ledger", "journal");
    private const string PayoutCurrency = "USD";

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<PayoutWorkflowState>()
            ?? throw new InvalidOperationException("PayoutWorkflowState not found in workflow context.");

        var ledgerId = _ids.Generate($"ledger|{state.SpvId}");
        var journalId = _ids.Generate($"journal|{state.PayoutId:N}");

        var entries = new List<JournalEntryInput>(state.Shares.Count * 2);
        foreach (var share in state.Shares)
        {
            var debitId = _ids.Generate($"debit|{state.PayoutId:N}|{share.ParticipantId}");
            entries.Add(new JournalEntryInput(
                debitId,
                state.SpvVaultId,
                share.Amount,
                PayoutCurrency,
                "Debit"));

            var creditId = _ids.Generate($"credit|{state.PayoutId:N}|{share.ParticipantId}");
            entries.Add(new JournalEntryInput(
                creditId,
                share.ParticipantVaultId,
                share.Amount,
                PayoutCurrency,
                "Credit"));
        }

        var command = new PostJournalEntriesCommand(ledgerId, journalId, entries);

        var result = await _dispatcher.DispatchSystemAsync(command, LedgerRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "PostLedgerJournal dispatch failed.");

        state.CurrentStep = PayoutExecutionSteps.PostLedgerJournal;
        context.SetState(state);

        return WorkflowStepResult.Success(journalId, result.EmittedEvents);
    }
}
