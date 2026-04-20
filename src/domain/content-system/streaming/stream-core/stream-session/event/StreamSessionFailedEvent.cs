using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed record StreamSessionFailedEvent(
    StreamSessionId SessionId,
    SessionTerminationReason Reason,
    Timestamp FailedAt) : DomainEvent;
