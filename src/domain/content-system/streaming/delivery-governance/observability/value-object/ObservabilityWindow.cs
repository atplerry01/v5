using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public readonly record struct ObservabilityWindow
{
    public Timestamp Start { get; }
    public Timestamp End { get; }

    public ObservabilityWindow(Timestamp start, Timestamp end)
    {
        Guard.Against(end.Value <= start.Value, "ObservabilityWindow end must be after start.");
        Start = start;
        End = end;
    }
}
