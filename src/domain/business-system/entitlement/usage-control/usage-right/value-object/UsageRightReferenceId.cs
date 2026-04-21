using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public readonly record struct UsageRightReferenceId
{
    public Guid Value { get; }

    public UsageRightReferenceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "UsageRightReferenceId cannot be empty.");
        Value = value;
    }
}
