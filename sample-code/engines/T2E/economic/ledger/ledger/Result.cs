namespace Whycespace.Engines.T2E.Economic.Ledger.Ledger;

public record LedgerResult(bool Success, string Message);

public sealed record LedgerEntryDto(
    string LedgerId,
    string EntryId,
    string AccountCode,
    decimal DebitAmount,
    decimal CreditAmount,
    string CurrencyCode);
