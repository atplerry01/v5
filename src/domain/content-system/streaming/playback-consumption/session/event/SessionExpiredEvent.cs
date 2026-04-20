using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionExpiredEvent(
    SessionId SessionId,
    Timestamp ExpiredAt) : DomainEvent;
