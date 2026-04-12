using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public sealed class CapitalPoolAggregate : AggregateRoot
{
    public PoolId PoolId { get; private set; }
    public Amount TotalCapital { get; private set; }
    public Currency Currency { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private CapitalPoolAggregate() { }

    public static CapitalPoolAggregate Create(PoolId poolId, Currency currency, Timestamp createdAt)
    {
        Guard.Against(string.IsNullOrWhiteSpace(currency.Code), "Currency code must not be empty.");

        var aggregate = new CapitalPoolAggregate();
        aggregate.RaiseDomainEvent(new PoolCreatedEvent(poolId, currency, createdAt));
        return aggregate;
    }

    public void AggregateCapital(Guid sourceAccountId, Amount amount)
    {
        Guard.Against(amount.Value <= 0, "Aggregated amount must be greater than zero.");

        var newTotal = new Amount(TotalCapital.Value + amount.Value);
        RaiseDomainEvent(new CapitalAggregatedEvent(PoolId, sourceAccountId, amount, newTotal));
    }

    public void ReduceCapital(Guid sourceAccountId, Amount amount)
    {
        Guard.Against(amount.Value <= 0, "Reduced amount must be greater than zero.");
        Guard.Against(amount.Value > TotalCapital.Value,
            $"Cannot reduce {amount.Value} from pool with {TotalCapital.Value} available.");

        var newTotal = new Amount(TotalCapital.Value - amount.Value);
        RaiseDomainEvent(new CapitalReducedEvent(PoolId, sourceAccountId, amount, newTotal));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PoolCreatedEvent e:
                PoolId = e.PoolId;
                Currency = e.Currency;
                CreatedAt = e.CreatedAt;
                TotalCapital = new Amount(0m);
                break;

            case CapitalAggregatedEvent e:
                TotalCapital = e.NewPoolTotal;
                break;

            case CapitalReducedEvent e:
                TotalCapital = e.NewPoolTotal;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(TotalCapital.Value < 0, "Pool total capital must not be negative.");
    }
}
