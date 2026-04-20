using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed class CanEndStreamSpecification : Specification<StreamAggregate>
{
    public override bool IsSatisfiedBy(StreamAggregate entity)
        => entity.Status == StreamStatus.Active || entity.Status == StreamStatus.Paused;
}
