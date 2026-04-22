using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public sealed record IngestSessionAuthenticatedEvent(
    [property: JsonPropertyName("AggregateId")] IngestSessionId SessionId,
    BroadcastRef BroadcastRef,
    IngestEndpoint Endpoint,
    Timestamp AuthenticatedAt) : DomainEvent;
