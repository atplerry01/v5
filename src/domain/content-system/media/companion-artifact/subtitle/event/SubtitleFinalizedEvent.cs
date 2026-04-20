using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Subtitle;

public sealed record SubtitleFinalizedEvent(
    SubtitleId SubtitleId,
    Timestamp FinalizedAt) : DomainEvent;
