using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public sealed class CanCompleteMediaProcessingSpecification : Specification<MediaProcessingAggregate>
{
    public override bool IsSatisfiedBy(MediaProcessingAggregate entity)
        => entity.Status == MediaProcessingStatus.Running;
}
