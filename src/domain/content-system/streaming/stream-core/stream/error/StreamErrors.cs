using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public static class StreamErrors
{
    public static DomainException StreamAlreadyActive()
        => new("Stream is already active.");

    public static DomainException StreamNotActive()
        => new("Stream is not active.");

    public static DomainException StreamNotPaused()
        => new("Stream is not paused.");

    public static DomainException StreamAlreadyEnded()
        => new("Stream is already ended.");

    public static DomainException StreamNotEnded()
        => new("Stream is not ended.");

    public static DomainException StreamAlreadyArchived()
        => new("Stream is already archived.");

    public static DomainException InvalidTransition(StreamStatus from, StreamStatus to)
        => new($"Invalid stream transition: {from} → {to}.");
}
