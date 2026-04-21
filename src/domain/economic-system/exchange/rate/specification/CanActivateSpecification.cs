using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public sealed class CanActivateSpecification : Specification<ExchangeRateAggregate>
{
    public override bool IsSatisfiedBy(ExchangeRateAggregate rate)
    {
        if (rate.Status != ExchangeRateStatus.Defined) return false;
        if (rate.RateValue.Value <= 0m) return false;

        return true;
    }
}
