using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed record StreamSessionActivatedEvent(
    StreamSessionId SessionId,
    Timestamp ActivatedAt) : DomainEvent;
