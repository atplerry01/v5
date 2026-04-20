using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed class CanEnablePlaybackSpecification : Specification<PlaybackAggregate>
{
    public override bool IsSatisfiedBy(PlaybackAggregate entity)
        => entity.Status == PlaybackStatus.Created || entity.Status == PlaybackStatus.Disabled;
}
