using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed record StreamSessionSuspendedEvent(
    StreamSessionId SessionId,
    Timestamp SuspendedAt) : DomainEvent;
