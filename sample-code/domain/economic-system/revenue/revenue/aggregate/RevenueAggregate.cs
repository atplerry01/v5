using Whycespace.Domain.EconomicSystem.Ledger.Obligation;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed class RevenueAggregate : AggregateRoot
{
    public Guid SettlementId { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public Money RecognizedAmount { get; private set; } = null!;

    public Money DeferredAmount => TotalAmount - RecognizedAmount;

    public static RevenueAggregate Create(Guid id, Guid settlementId, Money amount)
    {
        EnsureNotNull(amount, nameof(amount));

        var aggregate = new RevenueAggregate
        {
            Id = id,
            SettlementId = settlementId,
            TotalAmount = amount,
            RecognizedAmount = Money.Zero(amount.Currency)
        };

        return aggregate;
    }

    public void RecognizeRevenue(
        Money amount,
        ObligationStatus obligationStatus,
        RevenueInvariantService invariantService)
    {
        var newRecognized = RecognizedAmount + amount;

        var result = invariantService.Validate(TotalAmount, newRecognized, obligationStatus);

        if (!result.IsValid)
            throw new RevenueException(result.Error!);

        RecognizedAmount = newRecognized;

        RaiseDomainEvent(new RevenueRecognizedEvent(
            Id,
            amount.Amount,
            amount.Currency.Code));
    }

    public void ReverseRevenue(Money amount)
    {
        if (amount > RecognizedAmount)
            throw new RevenueException("Cannot reverse more than recognized");

        RecognizedAmount -= amount;

        RaiseDomainEvent(new RevenueReversedEvent(
            Id,
            amount.Amount,
            amount.Currency.Code));
    }

    private static void EnsureNotNull(object value, string name)
    {
        if (value is null)
            throw new ArgumentNullException(name);
    }
}
