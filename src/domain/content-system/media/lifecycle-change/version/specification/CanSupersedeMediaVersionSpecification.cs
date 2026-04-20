using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public sealed class CanSupersedeMediaVersionSpecification : Specification<MediaVersionAggregate>
{
    public override bool IsSatisfiedBy(MediaVersionAggregate entity)
        => entity.Status == MediaVersionStatus.Active;
}
