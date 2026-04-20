using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionResumedEvent(
    SessionId SessionId,
    Timestamp ResumedAt) : DomainEvent;
