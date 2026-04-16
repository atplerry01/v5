using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Version;

public static class AssetVersionErrors
{
    public static DomainException InvalidVersionNumber() => new("Version components must be non-negative integers.");
    public static DomainException InvalidAssetRef() => new("Version asset reference must be non-empty.");
    public static DomainException AlreadyPromoted() => new("Version is already promoted.");
    public static DomainException AlreadyRetired() => new("Version is retired.");
    public static DomainException CannotPromoteNonDraft(AssetVersionStatus status) =>
        new($"Only Draft versions may be promoted; current status is {status}.");
    public static DomainException SupersedingNotPromoted() =>
        new("Only a Promoted version may be superseded.");
    public static DomainInvariantViolationException AssetMissing() =>
        new("Invariant violated: version must reference a media asset.");
}
