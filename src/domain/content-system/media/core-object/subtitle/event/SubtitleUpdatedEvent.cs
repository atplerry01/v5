using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed record SubtitleUpdatedEvent(
    SubtitleId SubtitleId,
    SubtitleOutputRef OutputRef,
    Timestamp UpdatedAt) : DomainEvent;
