using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public sealed class HasContractSpecification : Specification<PricingAggregate>
{
    public override bool IsSatisfiedBy(PricingAggregate pricing) =>
        pricing.ContractId != Guid.Empty;
}
