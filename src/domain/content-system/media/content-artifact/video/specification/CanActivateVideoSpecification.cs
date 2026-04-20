using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public sealed class CanActivateVideoSpecification : Specification<VideoAggregate>
{
    public override bool IsSatisfiedBy(VideoAggregate entity)
        => entity.Status == VideoStatus.Draft;
}
