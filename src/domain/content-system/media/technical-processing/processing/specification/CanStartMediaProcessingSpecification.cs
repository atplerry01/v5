using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed class CanStartMediaProcessingSpecification : Specification<MediaProcessingAggregate>
{
    public override bool IsSatisfiedBy(MediaProcessingAggregate entity)
        => entity.Status == MediaProcessingStatus.Requested;
}
