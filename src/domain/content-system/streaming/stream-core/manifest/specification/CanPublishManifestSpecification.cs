using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed class CanPublishManifestSpecification : Specification<ManifestAggregate>
{
    public override bool IsSatisfiedBy(ManifestAggregate entity)
        => entity.Status == ManifestStatus.Created;
}
