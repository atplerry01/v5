using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public readonly record struct LatencyMeasurement
{
    public long Milliseconds { get; }

    public LatencyMeasurement(long milliseconds)
    {
        Guard.Against(milliseconds < 0, "LatencyMeasurement cannot be negative.");
        Milliseconds = milliseconds;
    }

    public override string ToString() => $"{Milliseconds} ms";
}
