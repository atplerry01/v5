using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed class TreasuryAggregate : AggregateRoot
{
    public TreasuryId TreasuryId { get; private set; }
    public Amount Balance { get; private set; }
    public Currency Currency { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private TreasuryAggregate() { }

    public static TreasuryAggregate Create(
        TreasuryId treasuryId,
        Currency currency,
        Timestamp createdAt)
    {
        var aggregate = new TreasuryAggregate();

        aggregate.RaiseDomainEvent(new TreasuryCreatedEvent(
            treasuryId,
            currency,
            createdAt));

        return aggregate;
    }

    public void AllocateFunds(Amount amount)
    {
        if (amount.Value <= 0)
            throw TreasuryErrors.InvalidAmount();

        if (amount.Value > Balance.Value)
            throw TreasuryErrors.InsufficientTreasuryFunds(amount, Balance);

        var newBalance = new Amount(Balance.Value - amount.Value);

        RaiseDomainEvent(new TreasuryFundAllocatedEvent(TreasuryId, amount, newBalance));
    }

    public void ReleaseFunds(Amount amount)
    {
        if (amount.Value <= 0)
            throw TreasuryErrors.InvalidAmount();

        var newBalance = new Amount(Balance.Value + amount.Value);

        RaiseDomainEvent(new TreasuryFundReleasedEvent(TreasuryId, amount, newBalance));
    }

    protected override void Apply(object @event)
    {
        switch (@event)
        {
            case TreasuryCreatedEvent e:
                TreasuryId = e.TreasuryId;
                Balance = new Amount(0);
                Currency = e.Currency;
                CreatedAt = e.CreatedAt;
                break;

            case TreasuryFundAllocatedEvent e:
                Balance = e.NewBalance;
                break;

            case TreasuryFundReleasedEvent e:
                Balance = e.NewBalance;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Balance.Value < 0, TreasuryErrors.NegativeTreasuryBalance().Message);
    }
}
