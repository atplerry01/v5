using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public sealed record IngestSessionResumedEvent(
    [property: JsonPropertyName("AggregateId")] IngestSessionId SessionId,
    Timestamp ResumedAt) : DomainEvent;
