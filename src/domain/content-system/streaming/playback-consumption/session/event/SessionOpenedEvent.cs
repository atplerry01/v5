using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed record SessionOpenedEvent(
    SessionId SessionId,
    StreamRef StreamRef,
    SessionWindow Window,
    Timestamp OpenedAt) : DomainEvent;
