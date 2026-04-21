using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed record TranscriptUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] TranscriptId TranscriptId,
    TranscriptOutputRef OutputRef,
    Timestamp UpdatedAt) : DomainEvent;
