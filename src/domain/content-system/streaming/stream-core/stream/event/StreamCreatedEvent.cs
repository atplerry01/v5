using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamCreatedEvent(
    [property: JsonPropertyName("AggregateId")] StreamId StreamId,
    StreamMode Mode,
    StreamType Type,
    Timestamp CreatedAt) : DomainEvent;
