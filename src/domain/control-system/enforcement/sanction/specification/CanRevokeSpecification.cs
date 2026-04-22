using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Sanction;

public sealed class CanRevokeSpecification : Specification<SanctionAggregate>
{
    public override bool IsSatisfiedBy(SanctionAggregate sanction) =>
        sanction.Status == SanctionStatus.Issued || sanction.Status == SanctionStatus.Active;
}
