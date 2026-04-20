using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed class CanStartLiveStreamSpecification : Specification<LiveStreamAggregate>
{
    public override bool IsSatisfiedBy(LiveStreamAggregate entity)
        => entity.Status == LiveStreamStatus.Created
           || entity.Status == LiveStreamStatus.Scheduled;
}
