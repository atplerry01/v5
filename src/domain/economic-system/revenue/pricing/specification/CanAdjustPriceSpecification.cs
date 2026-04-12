using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public sealed class CanAdjustPriceSpecification : Specification<PricingAggregate>
{
    public override bool IsSatisfiedBy(PricingAggregate pricing)
    {
        return pricing.ContractId != Guid.Empty
            && pricing.Price.Value > 0m;
    }
}
