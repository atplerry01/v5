using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed record TranscriptArchivedEvent(
    TranscriptId TranscriptId,
    Timestamp ArchivedAt) : DomainEvent;
