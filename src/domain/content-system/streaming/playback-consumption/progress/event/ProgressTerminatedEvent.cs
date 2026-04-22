using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public sealed record ProgressTerminatedEvent(
    [property: JsonPropertyName("AggregateId")] ProgressId ProgressId,
    Timestamp TerminatedAt) : DomainEvent;
