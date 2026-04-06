using Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Domain.Economic;

namespace Whycespace.Runtime.Engine.Domain.Economic;

/// <summary>
/// Runtime implementation of IRevenueAggregateFactory — bridges to domain RevenueAggregate.Create().
/// </summary>
public sealed class RevenueAggregateFactory : IRevenueAggregateFactory
{
    public object Create(Guid id, Guid settlementId, decimal amount, string currencyCode)
    {
        var currency = new Currency(currencyCode);
        var money = new Money(amount, currency);
        return RevenueAggregate.Create(id, settlementId, money);
    }
}

/// <summary>
/// Runtime implementation of IDistributionAggregateFactory — bridges to domain DistributionAggregate.Create().
/// </summary>
public sealed class DistributionAggregateFactory : IDistributionAggregateFactory
{
    public object Create(Guid id, Guid revenueId, decimal amount, string currencyCode)
    {
        var currency = new Currency(currencyCode);
        var money = new Money(amount, currency);
        return DistributionAggregate.Create(id, revenueId, money);
    }
}
