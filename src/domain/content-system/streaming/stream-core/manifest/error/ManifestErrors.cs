using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public static class ManifestErrors
{
    public static DomainException ManifestArchived()
        => new("Cannot mutate an archived manifest.");

    public static DomainException ManifestRetired()
        => new("Cannot publish or update a retired manifest.");

    public static DomainException AlreadyPublished()
        => new("Manifest at this version is already published.");

    public static DomainException CannotUpdateUnpublished()
        => new("Cannot update a manifest that has not been published.");

    public static DomainException AlreadyRetired()
        => new("Manifest is already retired.");

    public static DomainException AlreadyArchived()
        => new("Manifest is already archived.");

    public static DomainException InvalidRetirementReason()
        => new("Manifest retirement reason cannot be empty.");

    public static DomainInvariantViolationException OrphanedManifest()
        => new("Manifest must reference a valid source.");
}
