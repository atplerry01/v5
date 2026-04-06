namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

/// <summary>
/// Topic: whyce.economic.ledger.entry-recorded
/// Command: LedgerRecordCommand
/// Normalized: single Amount + EntryType (Debit/Credit). No duplicate numeric fields.
/// </summary>
public sealed record LedgerEntryRecordedEvent(
    Guid LedgerId,
    Guid AccountId,
    decimal Amount,
    string EntryType,
    string CurrencyCode = "") : DomainEvent;
