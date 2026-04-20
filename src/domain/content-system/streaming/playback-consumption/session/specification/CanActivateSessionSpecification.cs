using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed class CanActivateSessionSpecification : Specification<SessionAggregate>
{
    public override bool IsSatisfiedBy(SessionAggregate entity)
        => entity.Status == SessionStatus.Opened;
}
