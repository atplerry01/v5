using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public readonly record struct ContactPointId
{
    public Guid Value { get; }

    public ContactPointId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ContactPointId cannot be empty.");
        Value = value;
    }
}
