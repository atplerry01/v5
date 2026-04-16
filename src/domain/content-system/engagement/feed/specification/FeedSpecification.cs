using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Feed;

public sealed class FeedSpecification : Specification<FeedItem>
{
    public override bool IsSatisfiedBy(FeedItem entity) =>
        entity is not null && !string.IsNullOrWhiteSpace(entity.ItemRef) && entity.Rank >= 0;
}
