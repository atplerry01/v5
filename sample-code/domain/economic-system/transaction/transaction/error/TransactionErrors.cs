namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public static class TransactionErrors
{
    public static DomainException SameWallet(Guid walletId) =>
        new("TRANSACTION_SAME_WALLET", $"Source and destination wallet cannot be the same: {walletId}.");

    public static DomainException InvalidAmount() =>
        new("TRANSACTION_INVALID_AMOUNT", "TransactionAggregate amount must be positive.");

    public static DomainException AlreadyCompleted(Guid transactionId) =>
        new("TRANSACTION_ALREADY_COMPLETED", $"TransactionAggregate {transactionId} is already completed.");

    public static DomainException AlreadyFailed(Guid transactionId) =>
        new("TRANSACTION_ALREADY_FAILED", $"TransactionAggregate {transactionId} has already failed.");
}
