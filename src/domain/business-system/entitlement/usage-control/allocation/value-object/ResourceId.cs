using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public readonly record struct ResourceId
{
    public Guid Value { get; }

    public ResourceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ResourceId cannot be empty.");
        Value = value;
    }
}
