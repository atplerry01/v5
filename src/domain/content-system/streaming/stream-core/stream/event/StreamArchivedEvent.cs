using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed record StreamArchivedEvent(
    StreamId StreamId,
    Timestamp ArchivedAt) : DomainEvent;
