using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public sealed class CanUpdatePositionSpecification : Specification<ProgressAggregate>
{
    public override bool IsSatisfiedBy(ProgressAggregate entity)
        => entity.Status != ProgressStatus.Terminated;
}
