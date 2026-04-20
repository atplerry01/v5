using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public static class MediaMetadataErrors
{
    public static DomainException MetadataAlreadyFinalized()
        => new("Media metadata is finalized and cannot be modified.");

    public static DomainException DuplicateKey(string key)
        => new($"Media metadata key '{key}' already exists. Use update to change its value.");

    public static DomainException UnknownKey(string key)
        => new($"Media metadata key '{key}' does not exist.");

    public static DomainException EmptyMetadata()
        => new("Cannot finalize an empty media metadata set.");

    public static DomainInvariantViolationException OrphanedMetadata()
        => new("Media metadata must reference an owning asset.");
}
