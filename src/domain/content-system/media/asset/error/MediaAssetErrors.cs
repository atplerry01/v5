using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public static class MediaAssetErrors
{
    public static DomainException InvalidTitle() =>
        new("Media asset title must be non-empty.");

    public static DomainException TitleTooLong(int max) =>
        new($"Media asset title exceeds the permitted length of {max} characters.");

    public static DomainException DescriptionTooLong(int max) =>
        new($"Media asset description exceeds the permitted length of {max} characters.");

    public static DomainException InvalidContentDigest() =>
        new("Content digest must be a 64-character lowercase SHA-256 hex string.");

    public static DomainException InvalidStorageUri() =>
        new("Storage URI must be an absolute, well-formed URI.");

    public static DomainException InvalidStorageSize() =>
        new("Storage size in bytes must be greater than zero.");

    public static DomainException InvalidTag() =>
        new("Media tag must be non-empty.");

    public static DomainException TagTooLong(int max) =>
        new($"Media tag exceeds the permitted length of {max} characters.");

    public static DomainException InvalidAssetId() =>
        new("Media asset id must not be empty.");

    public static DomainException InvalidOwner() =>
        new("Media asset owner reference must be non-empty.");

    public static DomainException InvalidTransition(MediaAssetStatus from, MediaAssetStatus to) =>
        new($"Illegal media asset state transition from {from} to {to}.");

    public static DomainException AssetNotRegistered() =>
        new("Media asset has not been registered.");

    public static DomainException AssetAlreadyPublished() =>
        new("Media asset is already published.");

    public static DomainException AssetNotPublished() =>
        new("Media asset is not currently published.");

    public static DomainException AssetArchived() =>
        new("Media asset is archived and cannot be modified.");

    public static DomainException CannotArchiveFromStatus(MediaAssetStatus status) =>
        new($"Media asset cannot be archived from status {status}.");

    public static DomainException CannotPublishFromStatus(MediaAssetStatus status) =>
        new($"Media asset cannot be published from status {status}; must be Available.");

    public static DomainException CannotProcessFromStatus(MediaAssetStatus status) =>
        new($"Media asset cannot begin processing from status {status}; must be Draft.");

    public static DomainException CannotMarkAvailableFromStatus(MediaAssetStatus status) =>
        new($"Media asset cannot be marked Available from status {status}; must be Processing.");

    public static DomainInvariantViolationException TitleMissing() =>
        new("Invariant violated: a registered media asset must have a title.");

    public static DomainInvariantViolationException DigestMissing() =>
        new("Invariant violated: a registered media asset must have a content digest.");

    public static DomainInvariantViolationException StorageMissing() =>
        new("Invariant violated: a registered media asset must declare storage location.");

    public static DomainInvariantViolationException PublishedWithoutAvailability() =>
        new("Invariant violated: a published media asset must have completed availability.");

    public static DomainInvariantViolationException DuplicateTag(string tag) =>
        new($"Invariant violated: tag '{tag}' appears more than once on media asset.");
}
