using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed class CanDistributeSpecification : Specification<RevenueAggregate>
{
    public override bool IsSatisfiedBy(RevenueAggregate revenue) =>
        revenue.Status == RevenueStatus.Recognized;
}
