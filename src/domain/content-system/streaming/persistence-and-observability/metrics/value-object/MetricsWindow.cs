using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public readonly record struct MetricsWindow
{
    public Timestamp Start { get; }
    public Timestamp End { get; }

    public MetricsWindow(Timestamp start, Timestamp end)
    {
        Guard.Against(end.Value <= start.Value, "MetricsWindow end must be after start.");
        Start = start;
        End = end;
    }
}
