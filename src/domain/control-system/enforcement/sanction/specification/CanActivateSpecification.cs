using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Sanction;

public sealed class CanActivateSpecification : Specification<SanctionAggregate>
{
    public override bool IsSatisfiedBy(SanctionAggregate sanction) =>
        sanction.Status == SanctionStatus.Issued;
}
