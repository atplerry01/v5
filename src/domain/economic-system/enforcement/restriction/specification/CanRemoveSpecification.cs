using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Restriction;

public sealed class CanRemoveSpecification : Specification<RestrictionAggregate>
{
    public override bool IsSatisfiedBy(RestrictionAggregate restriction) =>
        restriction.Status == RestrictionStatus.Applied;
}
