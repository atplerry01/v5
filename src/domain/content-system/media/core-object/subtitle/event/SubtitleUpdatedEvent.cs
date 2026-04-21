using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed record SubtitleUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] SubtitleId SubtitleId,
    SubtitleOutputRef OutputRef,
    Timestamp UpdatedAt) : DomainEvent;
