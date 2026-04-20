using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Playback;

public sealed class CanEnablePlaybackSpecification : Specification<PlaybackAggregate>
{
    public override bool IsSatisfiedBy(PlaybackAggregate entity)
        => entity.Status == PlaybackStatus.Created || entity.Status == PlaybackStatus.Disabled;
}
