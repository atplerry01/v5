using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public readonly record struct LiveStreamId
{
    public Guid Value { get; }

    public LiveStreamId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "LiveStreamId cannot be empty.");
        Value = value;
    }
}
