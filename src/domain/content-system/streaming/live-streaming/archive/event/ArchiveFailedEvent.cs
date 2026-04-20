using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveFailedEvent(
    ArchiveId ArchiveId,
    ArchiveFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
