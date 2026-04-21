using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveFailedEvent(
    [property: JsonPropertyName("AggregateId")] ArchiveId ArchiveId,
    ArchiveFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
