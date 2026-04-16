using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public sealed class PayoutSpecification : Specification<ContentPayoutStatus>
{
    public override bool IsSatisfiedBy(ContentPayoutStatus entity) =>
        entity == ContentPayoutStatus.Calculated || entity == ContentPayoutStatus.Approved;

    public void EnsureApprovable(ContentPayoutStatus status)
    {
        if (status == ContentPayoutStatus.Approved) throw PayoutErrors.AlreadyApproved();
        if (status != ContentPayoutStatus.Calculated) throw PayoutErrors.NotCalculated();
    }

    public void EnsureSettleable(ContentPayoutStatus status)
    {
        if (status == ContentPayoutStatus.Settled) throw PayoutErrors.AlreadySettled();
        if (status != ContentPayoutStatus.Approved) throw PayoutErrors.NotApproved();
    }
}
