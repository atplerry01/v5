using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public readonly record struct SegmentSequenceNumber : IComparable<SegmentSequenceNumber>
{
    public long Value { get; }

    public SegmentSequenceNumber(long value)
    {
        Guard.Against(value < 0, "SegmentSequenceNumber must be >= 0.");
        Value = value;
    }

    public int CompareTo(SegmentSequenceNumber other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
