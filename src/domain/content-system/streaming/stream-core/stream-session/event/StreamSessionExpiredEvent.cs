using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed record StreamSessionExpiredEvent(
    StreamSessionId SessionId,
    Timestamp ExpiredAt) : DomainEvent;
