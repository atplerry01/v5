using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Subscription;

public sealed class SubscriptionSpecification : Specification<SubscriptionStatus>
{
    public override bool IsSatisfiedBy(SubscriptionStatus entity) => entity == SubscriptionStatus.Active;

    public void EnsureActivatable(SubscriptionStatus status)
    {
        if (status == SubscriptionStatus.Active) throw SubscriptionErrors.AlreadyActive();
        if (status != SubscriptionStatus.Created) throw SubscriptionErrors.CannotActivateFromStatus(status);
    }

    public void EnsureRenewable(SubscriptionStatus status)
    {
        if (status != SubscriptionStatus.Active) throw SubscriptionErrors.NotActive();
    }

    public void EnsurePeriod(Timestamp start, Timestamp end)
    {
        if (end.Value <= start.Value) throw SubscriptionErrors.InvalidPeriod();
    }
}
