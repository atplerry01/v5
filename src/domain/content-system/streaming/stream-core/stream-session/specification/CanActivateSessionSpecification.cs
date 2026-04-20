using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed class CanActivateSessionSpecification : Specification<StreamSessionAggregate>
{
    public override bool IsSatisfiedBy(StreamSessionAggregate entity)
        => entity.Status == SessionStatus.Opened;
}
