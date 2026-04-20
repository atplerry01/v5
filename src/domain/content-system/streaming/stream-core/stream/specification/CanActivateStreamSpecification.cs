using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed class CanActivateStreamSpecification : Specification<StreamAggregate>
{
    public override bool IsSatisfiedBy(StreamAggregate entity)
        => entity.Status == StreamStatus.Created || entity.Status == StreamStatus.Paused;
}
