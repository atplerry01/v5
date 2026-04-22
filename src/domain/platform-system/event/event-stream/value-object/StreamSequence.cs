using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventStream;

public readonly record struct StreamSequence
{
    public long Value { get; }

    public StreamSequence(long value)
    {
        Guard.Against(value < 0, "StreamSequence cannot be negative.");
        Value = value;
    }
}
