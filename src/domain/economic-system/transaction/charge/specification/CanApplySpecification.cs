using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed class CanApplySpecification : Specification<ChargeAggregate>
{
    public override bool IsSatisfiedBy(ChargeAggregate charge) =>
        charge.Status == ChargeStatus.Calculated;
}
