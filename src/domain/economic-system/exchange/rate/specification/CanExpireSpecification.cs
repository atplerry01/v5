using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public sealed class CanExpireSpecification : Specification<ExchangeRateAggregate>
{
    public override bool IsSatisfiedBy(ExchangeRateAggregate rate)
    {
        return rate.Status == ExchangeRateStatus.Active;
    }
}
