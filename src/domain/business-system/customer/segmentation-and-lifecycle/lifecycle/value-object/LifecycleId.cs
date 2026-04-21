using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public readonly record struct LifecycleId
{
    public Guid Value { get; }

    public LifecycleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "LifecycleId cannot be empty.");
        Value = value;
    }
}
