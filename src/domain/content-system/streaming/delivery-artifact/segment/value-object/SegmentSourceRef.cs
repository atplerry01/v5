using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public readonly record struct SegmentSourceRef
{
    public Guid Value { get; }
    public SegmentSourceKind Kind { get; }

    public SegmentSourceRef(Guid value, SegmentSourceKind kind)
    {
        Guard.Against(value == Guid.Empty, "SegmentSourceRef value cannot be empty.");
        Value = value;
        Kind = kind;
    }
}
