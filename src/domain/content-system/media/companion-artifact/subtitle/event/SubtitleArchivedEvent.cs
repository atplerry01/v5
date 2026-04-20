using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Subtitle;

public sealed record SubtitleArchivedEvent(
    SubtitleId SubtitleId,
    Timestamp ArchivedAt) : DomainEvent;
