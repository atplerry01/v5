using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Path;

public sealed class CanActivatePathSpecification : Specification<RoutingPathAggregate>
{
    public override bool IsSatisfiedBy(RoutingPathAggregate path)
    {
        if (path.Status != RoutingPathStatus.Defined) return false;
        if (string.IsNullOrWhiteSpace(path.Conditions)) return false;
        if (path.Priority <= 0) return false;

        return true;
    }
}
