using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamCreatedEvent(
    StreamId StreamId,
    StreamMode Mode,
    StreamType Type,
    Timestamp CreatedAt) : DomainEvent;
