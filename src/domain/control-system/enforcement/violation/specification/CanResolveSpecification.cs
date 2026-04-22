using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Violation;

public sealed class CanResolveSpecification : Specification<ViolationAggregate>
{
    public override bool IsSatisfiedBy(ViolationAggregate violation)
    {
        return violation.Status == ViolationStatus.Acknowledged;
    }
}
