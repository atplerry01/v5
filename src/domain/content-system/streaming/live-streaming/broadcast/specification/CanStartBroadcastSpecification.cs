using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed class CanStartBroadcastSpecification : Specification<BroadcastAggregate>
{
    public override bool IsSatisfiedBy(BroadcastAggregate entity)
        => entity.Status == BroadcastStatus.Created
           || entity.Status == BroadcastStatus.Scheduled;
}
