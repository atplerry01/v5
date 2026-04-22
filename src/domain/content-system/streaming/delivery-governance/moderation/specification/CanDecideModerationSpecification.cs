using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

public sealed class CanDecideModerationSpecification : Specification<ModerationAggregate>
{
    public override bool IsSatisfiedBy(ModerationAggregate entity)
        => entity.Status == ModerationStatus.InReview;
}
