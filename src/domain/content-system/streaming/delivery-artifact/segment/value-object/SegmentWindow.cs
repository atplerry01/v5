using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public readonly record struct SegmentWindow
{
    public Timestamp Start { get; }
    public Timestamp End { get; }

    public SegmentWindow(Timestamp start, Timestamp end)
    {
        Guard.Against(end.Value <= start.Value, "SegmentWindow end must be after start.");
        Start = start;
        End = end;
    }
}
