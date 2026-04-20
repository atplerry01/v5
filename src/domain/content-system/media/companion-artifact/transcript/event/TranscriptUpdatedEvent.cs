using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Transcript;

public sealed record TranscriptUpdatedEvent(
    TranscriptId TranscriptId,
    TranscriptOutputRef OutputRef,
    Timestamp UpdatedAt) : DomainEvent;
