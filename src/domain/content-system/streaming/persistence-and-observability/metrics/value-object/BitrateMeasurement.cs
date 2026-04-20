using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public readonly record struct BitrateMeasurement
{
    public long BitsPerSecond { get; }

    public BitrateMeasurement(long bitsPerSecond)
    {
        Guard.Against(bitsPerSecond < 0, "BitrateMeasurement cannot be negative.");
        BitsPerSecond = bitsPerSecond;
    }

    public override string ToString() => $"{BitsPerSecond} bps";
}
