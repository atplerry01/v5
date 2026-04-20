using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.Control.Access;

public sealed class CanRevokeAccessSpecification : Specification<StreamAccessAggregate>
{
    public override bool IsSatisfiedBy(StreamAccessAggregate entity)
        => entity.Status != AccessStatus.Revoked && entity.Status != AccessStatus.Expired;
}
