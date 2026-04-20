using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveStartedEvent(
    ArchiveId ArchiveId,
    StreamRef StreamRef,
    StreamSessionRef? SessionRef,
    Timestamp StartedAt) : DomainEvent;
