using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public readonly record struct SegmentId
{
    public Guid Value { get; }

    public SegmentId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SegmentId cannot be empty.");
        Value = value;
    }
}
