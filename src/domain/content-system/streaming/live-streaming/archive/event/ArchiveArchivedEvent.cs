using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ArchiveId ArchiveId,
    Timestamp ArchivedAt) : DomainEvent;
