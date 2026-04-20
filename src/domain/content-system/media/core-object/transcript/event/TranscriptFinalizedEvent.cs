using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed record TranscriptFinalizedEvent(
    TranscriptId TranscriptId,
    Timestamp FinalizedAt) : DomainEvent;
