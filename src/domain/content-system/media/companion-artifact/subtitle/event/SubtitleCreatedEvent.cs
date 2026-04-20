using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Subtitle;

public sealed record SubtitleCreatedEvent(
    SubtitleId SubtitleId,
    MediaAssetRef AssetRef,
    MediaFileRef? SourceFileRef,
    SubtitleFormat Format,
    SubtitleLanguage Language,
    SubtitleWindow? Window,
    Timestamp CreatedAt) : DomainEvent;
