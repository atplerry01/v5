using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

public sealed class CanExpireSpecification : Specification<SanctionAggregate>
{
    public override bool IsSatisfiedBy(SanctionAggregate sanction) =>
        sanction.Status == SanctionStatus.Active;
}
