using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed record SubtitleArchivedEvent(
    [property: JsonPropertyName("AggregateId")] SubtitleId SubtitleId,
    Timestamp ArchivedAt) : DomainEvent;
