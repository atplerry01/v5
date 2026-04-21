using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public readonly record struct UsageRightId
{
    public Guid Value { get; }

    public UsageRightId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "UsageRightId cannot be empty.");
        Value = value;
    }
}
