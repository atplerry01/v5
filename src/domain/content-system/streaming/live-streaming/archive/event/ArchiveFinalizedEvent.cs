using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveFinalizedEvent(
    [property: JsonPropertyName("AggregateId")] ArchiveId ArchiveId,
    Timestamp FinalizedAt) : DomainEvent;
