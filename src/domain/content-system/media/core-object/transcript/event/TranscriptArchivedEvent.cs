using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed record TranscriptArchivedEvent(
    [property: JsonPropertyName("AggregateId")] TranscriptId TranscriptId,
    Timestamp ArchivedAt) : DomainEvent;
