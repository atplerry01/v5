using Whycespace.Domain.EconomicSystem.Ledger.Ledger;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementAggregate : AggregateRoot
{
    public Guid PayeeIdentityId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public SettlementStatus Status { get; private set; } = SettlementStatus.Created;

    private readonly HashSet<Guid> _processedTransactions = new();

    public static SettlementAggregate Execute(Guid id, Guid payeeIdentityId, Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        var aggregate = new SettlementAggregate();
        aggregate.Id = id;
        aggregate.PayeeIdentityId = payeeIdentityId;
        aggregate.Amount = amount;
        aggregate.Status = SettlementStatus.Pending;
        aggregate.RaiseDomainEvent(new SettlementCreatedEvent(id));
        return aggregate;
    }

    public void ExecuteSettlement(
        Guid transactionId,
        IReadOnlyList<LedgerEntry> entries,
        Money amount,
        SettlementInvariantService invariantService)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(invariantService);

        if (Status == SettlementStatus.Completed)
            throw new SettlementException("Settlement already completed. Cannot re-settle.");

        if (_processedTransactions.Contains(transactionId))
            throw new SettlementException($"Duplicate settlement for transaction {transactionId}.");

        var result = invariantService.Validate(entries, amount);

        if (!result.IsValid)
            throw new SettlementException(result.Error!);

        _processedTransactions.Add(transactionId);
        Status = SettlementStatus.Completed;

        RaiseDomainEvent(new SettlementCompletedEvent(
            Id,
            amount.Amount,
            amount.Currency.Code));
    }
}
