using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Metadata;

public static class MetadataErrors
{
    public static DomainException InvalidKey() => new("Metadata key must be non-empty.");
    public static DomainException KeyTooLong(int max) => new($"Metadata key exceeds {max} characters.");
    public static DomainException ValueTooLong(int max) => new($"Metadata value exceeds {max} characters.");
    public static DomainException InvalidAssetRef() => new("Metadata asset reference must be non-empty.");
    public static DomainException AlreadyLocked() => new("Metadata record is locked.");
    public static DomainException TagAlreadyPresent(string tag) => new($"Tag '{tag}' already present.");
    public static DomainException TagNotPresent(string tag) => new($"Tag '{tag}' not present.");
    public static DomainInvariantViolationException AssetMissing() =>
        new("Invariant violated: metadata must be attached to an asset.");
}
