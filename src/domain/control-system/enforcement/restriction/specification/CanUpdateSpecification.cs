using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

public sealed class CanUpdateSpecification : Specification<RestrictionAggregate>
{
    public override bool IsSatisfiedBy(RestrictionAggregate restriction) =>
        restriction.Status == RestrictionStatus.Applied;
}
