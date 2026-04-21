using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveStartedEvent(
    [property: JsonPropertyName("AggregateId")] ArchiveId ArchiveId,
    StreamRef StreamRef,
    StreamSessionRef? SessionRef,
    Timestamp StartedAt) : DomainEvent;
