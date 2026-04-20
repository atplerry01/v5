using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamPausedEvent(
    StreamId StreamId,
    Timestamp PausedAt) : DomainEvent;
