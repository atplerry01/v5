using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed record SubtitleFinalizedEvent(
    [property: JsonPropertyName("AggregateId")] SubtitleId SubtitleId,
    Timestamp FinalizedAt) : DomainEvent;
