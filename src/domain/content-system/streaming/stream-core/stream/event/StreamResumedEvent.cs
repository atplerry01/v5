using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamResumedEvent(
    StreamId StreamId,
    Timestamp ResumedAt) : DomainEvent;
