using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed class CanCompleteSpecification : Specification<PayoutAggregate>
{
    public override bool IsSatisfiedBy(PayoutAggregate payout) =>
        payout.Status == PayoutStatus.Pending;
}
