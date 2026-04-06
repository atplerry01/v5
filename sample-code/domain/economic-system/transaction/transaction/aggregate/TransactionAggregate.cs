namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public class TransactionAggregate : AggregateRoot
{
    private SourceWalletId _sourceWalletId = null!;
    private DestinationWalletId _destinationWalletId = null!;
    private Amount _amount;
    private Currency _currency = null!;
    private TransactionStatus _status;
    private SettlementStatus _settlementStatus;

    public void Initiate(Guid id, SourceWalletId sourceWalletId, DestinationWalletId destinationWalletId, Amount amount, Currency currency)
    {
        EnsureInvariant(id != Guid.Empty, "TransactionId", "TransactionId cannot be empty.");
        ArgumentNullException.ThrowIfNull(sourceWalletId);
        ArgumentNullException.ThrowIfNull(destinationWalletId);
        ArgumentNullException.ThrowIfNull(currency);
        EnsureInvariant(sourceWalletId.Value != destinationWalletId.Value, "DifferentWallets", "Source and destination wallets must differ.");
        EnsureInvariant(amount.IsPositive, "PositiveAmount", "Transaction amount must be positive.");

        Id = id;
        _sourceWalletId = sourceWalletId;
        _destinationWalletId = destinationWalletId;
        _amount = amount;
        _currency = currency;
        _status = TransactionStatus.Pending;

        RaiseDomainEvent(new TransactionInitiatedEvent(id, sourceWalletId.Value, destinationWalletId.Value, amount.Value, currency.Code));
    }

    public void Approve()
    {
        EnsureInvariant(_status == TransactionStatus.Pending, "TransactionPending", "Only pending transactions can be approved.");

        var ledgerCorrelationId = EventId.Deterministic(Id, nameof(TransactionApprovedEvent), 1, "ledger").Value;
        RaiseDomainEvent(new TransactionApprovedEvent(Id, RequiresLedgerEntry: true, LedgerCorrelationId: ledgerCorrelationId));
    }

    public void Reject(string reason)
    {
        EnsureInvariant(_status == TransactionStatus.Pending, "TransactionPending", "Only pending transactions can be rejected.");

        _status = TransactionStatus.Failed;
        RaiseDomainEvent(new TransactionRejectedEvent(Id, reason));
    }

    public void Complete()
    {
        EnsureInvariant(_status == TransactionStatus.Pending, "TransactionPending", "Only pending transactions can be completed.");

        _status = TransactionStatus.Completed;
        _settlementStatus = SettlementStatus.Pending;
        RaiseDomainEvent(new TransactionCompletedEvent(Id));
    }

    public void Settle()
    {
        EnsureInvariant(_status == TransactionStatus.Completed, "TransactionCompleted", "Only completed transactions can be settled.");
        EnsureInvariant(_settlementStatus == SettlementStatus.Pending, "SettlementPending", "Only pending settlements can be settled.");

        _settlementStatus = SettlementStatus.Settled;
        RaiseDomainEvent(new TransactionSettledEvent(Id));
    }
}
