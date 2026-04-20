using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public readonly record struct StreamSessionRef
{
    public Guid Value { get; }

    public StreamSessionRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StreamSessionRef cannot be empty.");
        Value = value;
    }
}
