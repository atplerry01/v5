using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public readonly record struct DropCount
{
    public long Value { get; }

    public DropCount(long value)
    {
        Guard.Against(value < 0, "DropCount cannot be negative.");
        Value = value;
    }
}
