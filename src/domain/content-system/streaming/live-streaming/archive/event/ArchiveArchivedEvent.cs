using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed record ArchiveArchivedEvent(
    ArchiveId ArchiveId,
    Timestamp ArchivedAt) : DomainEvent;
