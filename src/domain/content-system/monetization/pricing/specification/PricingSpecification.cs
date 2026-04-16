using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Pricing;

public sealed class PricingSpecification : Specification<PricingPlanStatus>
{
    public override bool IsSatisfiedBy(PricingPlanStatus entity) =>
        entity == PricingPlanStatus.Draft || entity == PricingPlanStatus.Published;

    public void EnsureMutable(PricingPlanStatus status)
    {
        if (status == PricingPlanStatus.Deprecated) throw PricingErrors.CannotMutateDeprecated();
    }

    public void EnsurePublishable(PricingPlanStatus status, bool hasPrice)
    {
        if (status == PricingPlanStatus.Published) throw PricingErrors.AlreadyPublished();
        if (status == PricingPlanStatus.Deprecated) throw PricingErrors.CannotMutateDeprecated();
        if (!hasPrice) throw PricingErrors.CannotPublishWithoutPrice();
    }
}
