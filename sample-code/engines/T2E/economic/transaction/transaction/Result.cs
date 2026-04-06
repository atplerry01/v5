namespace Whycespace.Engines.T2E.Economic.Transaction.Transaction;

public record TransactionResult(bool Success, string Message);

public sealed record TransactionDto(
    string TransactionId,
    string SourceWalletId,
    string DestinationWalletId,
    decimal Amount,
    string CurrencyCode,
    string Status);
