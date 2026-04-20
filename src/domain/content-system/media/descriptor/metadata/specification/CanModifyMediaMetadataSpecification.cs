using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed class CanModifyMediaMetadataSpecification : Specification<MediaMetadataAggregate>
{
    public override bool IsSatisfiedBy(MediaMetadataAggregate entity)
        => entity.Status == MediaMetadataStatus.Open;
}
