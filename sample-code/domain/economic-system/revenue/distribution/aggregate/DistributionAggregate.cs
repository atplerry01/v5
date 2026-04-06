namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionAggregate : AggregateRoot
{
    public Guid RevenueId { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public IReadOnlyList<DistributionAllocation> Allocations => _allocations;

    private readonly List<DistributionAllocation> _allocations = new();
    private readonly HashSet<Guid> _processedTransactions = new();

    public DistributionAggregate() { }

    public static DistributionAggregate Create(Guid id, Guid revenueId, Money amount)
    {
        if (id == Guid.Empty)
            throw new DistributionException("Invalid distribution id");

        if (revenueId == Guid.Empty)
            throw new DistributionException("Invalid revenue id");

        if (amount.IsZero || amount.IsNegative)
            throw new DistributionException("Invalid distribution amount");

        var aggregate = new DistributionAggregate
        {
            Id = id,
            RevenueId = revenueId,
            TotalAmount = amount
        };

        aggregate.RaiseDomainEvent(new DistributionCreatedEvent(id));

        return aggregate;
    }

    public void Distribute(
        Guid transactionId,
        IReadOnlyList<DistributionAllocation> allocations,
        DistributionInvariantService invariantService)
    {
        if (transactionId == Guid.Empty)
            throw new DistributionException("Invalid transaction id");

        if (_processedTransactions.Contains(transactionId))
            throw new DistributionException("Duplicate distribution");

        var result = invariantService.Validate(TotalAmount, allocations);

        if (!result.IsValid)
            throw new DistributionException(result.Error!);

        _processedTransactions.Add(transactionId);

        _allocations.Clear();
        _allocations.AddRange(allocations);

        RaiseDomainEvent(new DistributionExecutedEvent(
            Id,
            TotalAmount.Amount,
            TotalAmount.Currency.Code));
    }

    public void Clawback(Guid transactionId)
    {
        if (!_processedTransactions.Contains(transactionId))
            throw new DistributionException("Unknown transaction");

        RaiseDomainEvent(new DistributionClawbackEvent(Id, transactionId));
    }
}
