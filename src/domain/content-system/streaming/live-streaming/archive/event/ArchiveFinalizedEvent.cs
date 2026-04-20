using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveFinalizedEvent(
    ArchiveId ArchiveId,
    Timestamp FinalizedAt) : DomainEvent;
