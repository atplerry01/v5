using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public readonly record struct UsageRightSubjectId
{
    public Guid Value { get; }

    public UsageRightSubjectId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "UsageRightSubjectId cannot be empty.");
        Value = value;
    }
}
