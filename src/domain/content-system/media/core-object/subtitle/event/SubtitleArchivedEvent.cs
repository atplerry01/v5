using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed record SubtitleArchivedEvent(
    SubtitleId SubtitleId,
    Timestamp ArchivedAt) : DomainEvent;
