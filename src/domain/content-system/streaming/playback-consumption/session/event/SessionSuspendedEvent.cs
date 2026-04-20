using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionSuspendedEvent(
    SessionId SessionId,
    Timestamp SuspendedAt) : DomainEvent;
