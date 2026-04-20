using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveCompletedEvent(
    ArchiveId ArchiveId,
    ArchiveOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
