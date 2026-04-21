using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamResumedEvent(
    [property: JsonPropertyName("AggregateId")] StreamId StreamId,
    Timestamp ResumedAt) : DomainEvent;
