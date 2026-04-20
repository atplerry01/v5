using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public readonly record struct ViewerCount
{
    public long Value { get; }

    public ViewerCount(long value)
    {
        Guard.Against(value < 0, "ViewerCount cannot be negative.");
        Value = value;
    }
}
