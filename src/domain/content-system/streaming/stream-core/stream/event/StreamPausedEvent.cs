using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamPausedEvent(
    [property: JsonPropertyName("AggregateId")] StreamId StreamId,
    Timestamp PausedAt) : DomainEvent;
