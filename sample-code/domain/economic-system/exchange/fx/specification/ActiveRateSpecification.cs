namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

/// <summary>
/// Specification: FX rate must be active and not superseded.
/// </summary>
public sealed class ActiveRateSpecification
{
    public bool IsSatisfiedBy(FxRateAggregate rate) =>
        rate.Status == FxRateStatus.Active;
}
