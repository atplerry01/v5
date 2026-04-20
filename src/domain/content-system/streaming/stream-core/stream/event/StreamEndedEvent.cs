using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamEndedEvent(
    StreamId StreamId,
    Timestamp EndedAt) : DomainEvent;
