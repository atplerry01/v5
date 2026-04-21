using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed record TranscriptCreatedEvent(
    [property: JsonPropertyName("AggregateId")] TranscriptId TranscriptId,
    MediaAssetRef AssetRef,
    MediaFileRef? SourceFileRef,
    TranscriptFormat Format,
    TranscriptLanguage Language,
    Timestamp CreatedAt) : DomainEvent;
