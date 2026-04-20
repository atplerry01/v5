using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed record TranscriptUpdatedEvent(
    TranscriptId TranscriptId,
    TranscriptOutputRef OutputRef,
    Timestamp UpdatedAt) : DomainEvent;
