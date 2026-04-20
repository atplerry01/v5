using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public readonly record struct StreamSessionId
{
    public Guid Value { get; }

    public StreamSessionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StreamSessionId cannot be empty.");
        Value = value;
    }
}
