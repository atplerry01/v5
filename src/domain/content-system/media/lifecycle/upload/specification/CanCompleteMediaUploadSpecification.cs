using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public sealed class CanCompleteMediaUploadSpecification : Specification<MediaUploadAggregate>
{
    public override bool IsSatisfiedBy(MediaUploadAggregate entity)
        => entity.Status == MediaUploadStatus.Processing;
}
