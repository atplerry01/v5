using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed record TranscriptCreatedEvent(
    TranscriptId TranscriptId,
    MediaAssetRef AssetRef,
    MediaFileRef? SourceFileRef,
    TranscriptFormat Format,
    TranscriptLanguage Language,
    Timestamp CreatedAt) : DomainEvent;
