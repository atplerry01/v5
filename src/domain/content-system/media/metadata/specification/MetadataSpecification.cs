using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Metadata;

public sealed class MetadataSpecification : Specification<MetadataField>
{
    public override bool IsSatisfiedBy(MetadataField entity) =>
        entity is not null && !string.IsNullOrWhiteSpace(entity.Key);

    public void EnsureMutable(MetadataStatus status)
    {
        if (status == MetadataStatus.Locked)
            throw MetadataErrors.AlreadyLocked();
    }
}
