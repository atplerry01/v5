using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed record SubtitleFinalizedEvent(
    SubtitleId SubtitleId,
    Timestamp FinalizedAt) : DomainEvent;
