using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed record StreamSessionClosedEvent(
    StreamSessionId SessionId,
    SessionTerminationReason Reason,
    Timestamp ClosedAt) : DomainEvent;
