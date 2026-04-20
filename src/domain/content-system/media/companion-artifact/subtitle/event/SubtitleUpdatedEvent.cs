using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Subtitle;

public sealed record SubtitleUpdatedEvent(
    SubtitleId SubtitleId,
    SubtitleOutputRef OutputRef,
    Timestamp UpdatedAt) : DomainEvent;
