using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public readonly record struct ErrorCount
{
    public long Value { get; }

    public ErrorCount(long value)
    {
        Guard.Against(value < 0, "ErrorCount cannot be negative.");
        Value = value;
    }
}
