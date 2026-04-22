using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public sealed class CanAbandonReplaySpecification : Specification<ReplayAggregate>
{
    public override bool IsSatisfiedBy(ReplayAggregate entity)
        => entity.Status != ReplayStatus.Completed && entity.Status != ReplayStatus.Abandoned;
}
