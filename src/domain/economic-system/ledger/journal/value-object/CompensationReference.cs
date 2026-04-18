namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

/// <summary>
/// Phase 7 T7.4 — correlates a compensating journal to the original it
/// reverses. The ledger remains append-only; the reference ties the new
/// balanced journal to the prior posted one for audit-trail reconstruction.
/// </summary>
public sealed record CompensationReference
{
    public Guid OriginalJournalId { get; }
    public string Reason { get; }

    public CompensationReference(Guid originalJournalId, string reason)
    {
        if (originalJournalId == Guid.Empty)
            throw new ArgumentException(
                "CompensationReference requires a non-empty OriginalJournalId.",
                nameof(originalJournalId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "CompensationReference requires a non-empty Reason.",
                nameof(reason));

        OriginalJournalId = originalJournalId;
        Reason = reason;
    }
}
