using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamEndedEvent(
    [property: JsonPropertyName("AggregateId")] StreamId StreamId,
    Timestamp EndedAt) : DomainEvent;
