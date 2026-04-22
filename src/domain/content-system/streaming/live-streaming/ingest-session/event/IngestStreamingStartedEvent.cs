using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public sealed record IngestStreamingStartedEvent(
    [property: JsonPropertyName("AggregateId")] IngestSessionId SessionId,
    Timestamp StartedAt) : DomainEvent;
