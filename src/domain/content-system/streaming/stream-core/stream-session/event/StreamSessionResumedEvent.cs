using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed record StreamSessionResumedEvent(
    StreamSessionId SessionId,
    Timestamp ResumedAt) : DomainEvent;
