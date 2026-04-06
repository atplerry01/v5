namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Engine-facing revenue aggregate factory contract.
/// Engines MUST NOT call static RevenueAggregate.Create() or DistributionAggregate.Create() directly.
/// The runtime provides implementations that delegate to domain factories.
/// </summary>
public interface IRevenueAggregateFactory
{
    /// <summary>
    /// Creates a RevenueAggregate with the given parameters.
    /// Returns the aggregate as IEventSource for engine context emission.
    /// </summary>
    object Create(Guid id, Guid settlementId, decimal amount, string currencyCode);
}

public interface IDistributionAggregateFactory
{
    /// <summary>
    /// Creates a DistributionAggregate with the given parameters.
    /// </summary>
    object Create(Guid id, Guid revenueId, decimal amount, string currencyCode);
}
