namespace Whycespace.Engines.T2E.Economic.Transaction.Transaction;

public record TransactionCommand(string Action, string EntityId, object Payload);

public sealed record InitiateTransactionCommand(
    string TransactionId,
    string SourceWalletId,
    string DestinationWalletId,
    decimal Amount,
    string CurrencyCode,
    string EnforcementDecision,
    string? EnforcementReasonCode,
    bool IsWithinLimits,
    bool IsSourceWalletActive,
    bool IsSourceWalletFrozen,
    bool IsDestinationWalletActive,
    bool IsDestinationWalletFrozen) : TransactionCommand("Initiate", TransactionId, null!);

public sealed record CompleteTransactionCommand(string TransactionId)
    : TransactionCommand("Complete", TransactionId, null!);

public sealed record RejectTransactionCommand(string TransactionId, string Reason)
    : TransactionCommand("Reject", TransactionId, null!);

public sealed record SettleTransactionCommand(string TransactionId)
    : TransactionCommand("Settle", TransactionId, null!);
