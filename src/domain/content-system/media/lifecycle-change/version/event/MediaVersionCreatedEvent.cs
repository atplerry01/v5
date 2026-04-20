using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public sealed record MediaVersionCreatedEvent(
    MediaVersionId VersionId,
    MediaAssetRef AssetRef,
    MediaVersionNumber VersionNumber,
    MediaFileRef FileRef,
    MediaVersionId? PreviousVersionId,
    Timestamp CreatedAt) : DomainEvent;
