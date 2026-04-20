using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public sealed class CanActivateImageSpecification : Specification<ImageAggregate>
{
    public override bool IsSatisfiedBy(ImageAggregate entity)
        => entity.Status == ImageStatus.Draft;
}
