namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

/// <summary>
/// Phase 7 T7.4 — classifies a journal as an original posting or a
/// compensating reversal. Compensating journals preserve the append-only
/// property of the ledger: no entry is ever deleted or mutated; reversal
/// is achieved by posting a new balanced journal whose net effect cancels
/// the original.
/// </summary>
public enum JournalKind
{
    Standard,
    Compensating
}
