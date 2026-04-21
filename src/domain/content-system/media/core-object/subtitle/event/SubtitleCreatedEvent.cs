using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed record SubtitleCreatedEvent(
    [property: JsonPropertyName("AggregateId")] SubtitleId SubtitleId,
    MediaAssetRef AssetRef,
    MediaFileRef? SourceFileRef,
    SubtitleFormat Format,
    SubtitleLanguage Language,
    SubtitleWindow? Window,
    Timestamp CreatedAt) : DomainEvent;
