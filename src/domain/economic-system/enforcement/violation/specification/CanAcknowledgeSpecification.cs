using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Violation;

public sealed class CanAcknowledgeSpecification : Specification<ViolationAggregate>
{
    public override bool IsSatisfiedBy(ViolationAggregate violation)
    {
        return violation.Status == ViolationStatus.Open;
    }
}
