namespace Whycespace.Engines.T2E.Economic.Ledger.Ledger;

public record LedgerCommand(string Action, string EntityId, object Payload);

public sealed record RecordLedgerEntryCommand(
    string LedgerId,
    string EntryId,
    string AccountCode,
    string AccountName,
    decimal DebitAmount,
    decimal CreditAmount,
    string CurrencyCode) : LedgerCommand("Record", LedgerId, null!);
