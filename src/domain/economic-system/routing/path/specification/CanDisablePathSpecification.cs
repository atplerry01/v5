using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Path;

public sealed class CanDisablePathSpecification : Specification<RoutingPathAggregate>
{
    public override bool IsSatisfiedBy(RoutingPathAggregate path)
    {
        return path.Status == RoutingPathStatus.Active;
    }
}
