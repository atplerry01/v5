using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionActivatedEvent(
    SessionId SessionId,
    Timestamp ActivatedAt) : DomainEvent;
