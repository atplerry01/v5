using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public static class ImageErrors
{
    public static DomainException ImageArchived()
        => new("Cannot mutate an archived image.");

    public static DomainException AlreadyActive()
        => new("Image is already active.");

    public static DomainException AlreadyArchived()
        => new("Image is already archived.");

    public static DomainInvariantViolationException OrphanedImage()
        => new("Image must reference an owning media asset.");
}
