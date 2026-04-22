using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public sealed record IngestSessionFailedEvent(
    [property: JsonPropertyName("AggregateId")] IngestSessionId SessionId,
    string FailureReason,
    Timestamp FailedAt) : DomainEvent;
