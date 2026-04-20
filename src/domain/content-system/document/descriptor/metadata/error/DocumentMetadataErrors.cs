using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public static class DocumentMetadataErrors
{
    public static DomainException MetadataAlreadyFinalized()
        => new("Document metadata is finalized and cannot be modified.");

    public static DomainException DuplicateKey(string key)
        => new($"Metadata key '{key}' already exists. Use update to change its value.");

    public static DomainException UnknownKey(string key)
        => new($"Metadata key '{key}' does not exist.");

    public static DomainException EmptyMetadata()
        => new("Cannot finalize an empty metadata set.");

    public static DomainInvariantViolationException OrphanedMetadata()
        => new("Document metadata must reference an owning document.");
}
