using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Bundle;

public static class DocumentBundleErrors
{
    public static DomainException BundleFinalized()
        => new("Bundle is finalized and cannot be modified.");

    public static DomainException BundleArchived()
        => new("Cannot mutate an archived bundle.");

    public static DomainException AlreadyArchived()
        => new("Bundle is already archived.");

    public static DomainException AlreadyFinalized()
        => new("Bundle is already finalized.");

    public static DomainException DuplicateMember()
        => new("Bundle already contains the requested member.");

    public static DomainException UnknownMember()
        => new("Bundle does not contain the requested member.");

    public static DomainException EmptyBundle()
        => new("Cannot finalize an empty bundle.");
}
