using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public sealed class CanVerifyIntegritySpecification : Specification<MediaFileAggregate>
{
    public override bool IsSatisfiedBy(MediaFileAggregate entity)
        => entity.RegistrationStatus == FileRegistrationStatus.Registered
           && entity.IntegrityStatus == FileIntegrityStatus.Unverified;
}
