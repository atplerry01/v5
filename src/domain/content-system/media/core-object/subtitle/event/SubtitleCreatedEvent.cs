using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed record SubtitleCreatedEvent(
    SubtitleId SubtitleId,
    MediaAssetRef AssetRef,
    MediaFileRef? SourceFileRef,
    SubtitleFormat Format,
    SubtitleLanguage Language,
    SubtitleWindow? Window,
    Timestamp CreatedAt) : DomainEvent;
