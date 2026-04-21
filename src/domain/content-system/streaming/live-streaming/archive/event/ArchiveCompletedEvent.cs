using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveCompletedEvent(
    [property: JsonPropertyName("AggregateId")] ArchiveId ArchiveId,
    ArchiveOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
