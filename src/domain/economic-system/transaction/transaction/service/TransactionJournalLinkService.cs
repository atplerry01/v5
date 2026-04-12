namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed class TransactionJournalLinkService
{
    /// <summary>
    /// Validates that a completed transaction has produced a journal.
    /// Every transaction MUST produce a journal — this is a critical cross-domain invariant.
    /// </summary>
    public bool ValidateJournalProduced(TransactionAggregate transaction) =>
        transaction.Status == TransactionStatus.Completed &&
        transaction.JournalId != Guid.Empty;
}
