using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public readonly record struct StreamId
{
    public Guid Value { get; }

    public StreamId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StreamId cannot be empty.");
        Value = value;
    }
}
